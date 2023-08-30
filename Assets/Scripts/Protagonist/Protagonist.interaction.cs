using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
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
		#endregion

		#region Fields
		private Interaction focusingObject;
		private Grabbable grabbingObject;
		private Coroutine grabbingCoroutine;
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

				if(grabbingObject)
					StartCoroutine(EndGrabbingCoroutine(grabbingObject));
				grabbingObject = value;
				if(grabbingObject)
					grabbingCoroutine = StartCoroutine(BeginGrabbingCoroutine(grabbingObject));

				UpdateFocusUi();
			}
		}

		private IEnumerator BeginGrabbingCoroutine(Grabbable target) {
			grabbingObject.SendMessage("OnGrabBegin");
			grabbingObject.transform.SetParent(eye.transform);
			yield return GrabbingCoroutine(target);
		}

		private IEnumerator GrabbingCoroutine(Grabbable target) {
			while(GrabbingObject == target) {
				yield return new WaitForFixedUpdate();
			}
		}

		private IEnumerator EndGrabbingCoroutine(Grabbable target) {
			if(grabbingCoroutine != null) {
				StopCoroutine(grabbingCoroutine);
				grabbingCoroutine = null;
			}
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