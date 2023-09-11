using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaniCore.UnityPlayground {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[Header("Interaction")]
		public new Camera camera;
		[SerializeField][Min(0)] protected float maxInteractionDistance;
		[SerializeField] protected LayerMask interactionLayerMask;
		[SerializeField] protected Image focusUi;
		[Serializable]
		public struct FocusUiMap {
			public Sprite normal;
			public Sprite hovering;
			public Sprite grabbing;
		}
		[SerializeField] protected FocusUiMap focusUiMap;
		[SerializeField][Range(0, 1)] protected float grabbingTransitionDuration;
		[SerializeField][Range(0, 1)] protected float grabbingEasingFactor;
		#endregion

		#region Fields
		private Interaction focusingObject;
		private Grabbable grabbingObject;
		private Coroutine grabbingCoroutine;
		private bool grabbingOrienting;
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		private void ValidateInteraction() {
			EditorApplication.delayCall += () => FocusUi = focusUiMap.hovering;
		}
#endif

		private void StartInteraction() {
			FocusingObject = null;
			FocusUi = focusUiMap.normal;
		}

		private void UpdateInteraction() {
			if(GrabbingObject == null) {
				RaycastHit hitInfo;
				bool isHit = Physics.Raycast(camera.ViewportPointToRay(Vector2.one * .5f), out hitInfo, maxInteractionDistance, interactionLayerMask);
				FocusingObject = isHit ? hitInfo.collider.GetComponent<Interaction>() : null;
			}
		}
		#endregion

		#region Functions
		public Sprite FocusUi {
			get => focusUi?.sprite;
			set {
				if(focusUi != null) {
					focusUi.sprite = value;
					if(value == null)
						focusUi.rectTransform.sizeDelta = Vector2.zero;
					else
						focusUi.SetNativeSize();
				}
			}
		}

		private void UpdateFocusUi() {
			if(GrabbingObject)
				FocusUi = focusUiMap.grabbing;
			else if(FocusingObject)
				FocusUi = focusUiMap.hovering;
			else
				FocusUi = focusUiMap.normal;
		}

		public Interaction FocusingObject {
			get => focusingObject;
			set {
				if(focusingObject == value)
					return;

				if(focusingObject)
					focusingObject.SendMessage("OnFocusLeave");
				focusingObject = value;
				if(focusingObject)
					focusingObject.SendMessage("OnFocusEnter");

				UpdateFocusUi();
			}
		}

		#region Grabbing
		public Grabbable GrabbingObject {
			get => grabbingObject;
			set {
				if(grabbingObject == value)
					return;

				if(grabbingObject) {
					if(grabbingCoroutine != null)
						StopCoroutine(grabbingCoroutine);
					StartCoroutine(EndGrabbingCoroutine(grabbingObject));
				}
				grabbingObject = value;
				if(grabbingObject)
					grabbingCoroutine = StartCoroutine(GrabCoroutine(grabbingObject));

				UpdateFocusUi();
			}
		}

		public bool GrabbingOrienting {
			get => grabbingObject != null && grabbingOrienting;
			set => grabbingOrienting = grabbingObject != null && value;
		}

		public void GrabbingOrientDelta(float delta) {
			delta *= orientingSpeed;
			float grabbingAzimuth = grabbingObject.transform.localRotation.eulerAngles.y * Mathf.PI / 180;
			grabbingAzimuth += delta;
			grabbingObject.transform.localRotation = Quaternion.Euler(0, grabbingAzimuth * 180 / Mathf.PI, 0);
		}

		private IEnumerator GrabCoroutine(Grabbable target) {
			yield return BeginGrabbingCoroutine(target);
			while(GrabbingObject == target) {
				yield return DuringGrabbingCoroutine(target);
				yield return new WaitForFixedUpdate();
			}
			yield return EndGrabbingCoroutine(target);
			grabbingCoroutine = null;
		}

		private IEnumerator BeginGrabbingCoroutine(Grabbable target) {
			target.SendMessage("OnGrabBegin");

			target.transform.SetParent(eye.transform, true);

			float grabbingDistance = Vector3.Distance(target.transform.position, eye.transform.position);
			float grabbingAzimuth = target.transform.localRotation.eulerAngles.y * Mathf.PI / 180;

			Vector3
				startPosition = target.transform.localPosition,
				endPosition = Vector3.forward * grabbingDistance;
			Quaternion
				startRotation = target.transform.localRotation,
				endRotation = Quaternion.Euler(0, grabbingAzimuth * 180 / Mathf.PI, 0);

			float startTime = Time.time;
			for(float t; (t = (Time.time - startTime) / grabbingTransitionDuration) < 1;) {
				t = MathUtility.Ease(t, grabbingEasingFactor);
				target.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
				target.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
				yield return new WaitForFixedUpdate();
			}
			target.transform.localPosition = endPosition;
			target.transform.localRotation = endRotation;

			yield break;
		}

		private IEnumerator DuringGrabbingCoroutine(Grabbable target) {
			yield break;
		}

		private IEnumerator EndGrabbingCoroutine(Grabbable target) {
			target.transform.SetParent(null, true);

			target.SendMessage("OnGrabEnd");

			yield break;
		}
		#endregion

		public void Interact() {
			if(GrabbingObject == null)
				FocusingObject?.SendMessage("OnInteract");
			else
				GrabbingObject = null;
		}
		#endregion
	}
}