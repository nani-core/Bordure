using UnityEngine;
using System.Collections;

namespace NaniCore.Stencil {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private FocusUi focus;
		#endregion

		#region Fields
		private GameObject lookingAtObject = null;
		private Transform grabbingObject;
		private bool grabbingOrienting;
		#endregion

		#region Interfaces
		public GameObject LookingAtObject => lookingAtObject;

		public Transform GrabbingObject {
			get => grabbingObject;
			set {
				if(grabbingObject == value)
					return;

				if(grabbingObject != null) {
					grabbingObject.transform.SetParent(null, true);
					grabbingObject.SendMessage("OnGrabEnd");
					PlaySfx(Profile.onDropSound);
				}

				grabbingObject = value;

				if(grabbingObject != null) {
					grabbingObject.SendMessage("OnGrabBegin");
					PlaySfx(Profile.onGrabSound);

					grabbingObject.transform.SetParent(eye.transform, true);
				}

				inputHandler.SetGrabbingActionEnabled(grabbingObject != null);
			}
		}

		public bool GrabbingOrienting {
			get => GrabbingObject != null && grabbingOrienting;
			set => grabbingOrienting = GrabbingObject != null && value;
		}

		public void GrabbingOrientDelta(float delta) {
			delta *= Profile.orientingSpeed;
			float grabbingAzimuth = GrabbingObject.localRotation.eulerAngles.y * Mathf.PI / 180;
			grabbingAzimuth += delta;
			GrabbingObject.localRotation = Quaternion.Euler(0, grabbingAzimuth * 180 / Mathf.PI, 0);
		}

		public void ResetGrabbingTransform() {
			if(GrabbingObject == null)
				return;

			StartCoroutine(GrabCoroutine(GrabbingObject));
		}

		public bool EyeCast(out RaycastHit hit) {
			return PhysicsUtility.Raycast(EyeRay, out hit, Profile.maxInteractionDistance, GameManager.Instance.GrabbingLayerMask, false);
		}
		#endregion

		#region Life cycle
		private void InitializeInteraction() {
			inputHandler = gameObject.EnsureComponent<ProtagonistInputHandler>();

			if(focus == null) {
				Debug.LogWarning("No FocusUi component found in the protagonist interaction UI prefab.", this);
			}
		}

		private void UpdateInteraction() {
			bool hasHit = EyeCast(out RaycastHit lookingHit);
			lookingAtObject = hasHit ? lookingHit.transform.gameObject : null;

			UpdateFocusUi();
		}
		#endregion

		#region Functions
		private void UpdateFocusUi() {
			if(focus == null)
				return;

			if(GameManager.Instance.HasValidLoopshapes)
				focus.CurrentStatus = FocusUi.Status.Hovering;
			else if(GrabbingObject)
				focus.CurrentStatus = FocusUi.Status.Grabbing;
			else
				focus.CurrentStatus = FocusUi.Status.Normal;
		}

		private IEnumerator GrabCoroutine(Transform target) {
			float grabbingDistance = Vector3.Distance(target.position, eye.transform.position);
			float grabbingAzimuth = target.localRotation.eulerAngles.y * Mathf.PI / 180;

			Vector3
				startPosition = target.localPosition,
				endPosition = Vector3.forward * grabbingDistance;
			Quaternion
				startRotation = target.localRotation,
				endRotation = Quaternion.Euler(0, grabbingAzimuth * 180 / Mathf.PI, 0);

			float startTime = Time.time;
			for(float t; (t = (Time.time - startTime) / Profile.grabbingTransitionDuration) < 1;) {
				t = MathUtility.Ease(t, Profile.grabbingEasingFactor);
				target.SetLocalPositionAndRotation(
					Vector3.Lerp(startPosition, endPosition, t),
					Quaternion.Slerp(startRotation, endRotation, t)
				);
				yield return new WaitForFixedUpdate();
			}
			target.SetLocalPositionAndRotation(endPosition, endRotation);
		}

		public void Interact() {
			if(GrabbingObject != null) {
				GrabbingObject = null;
			}
			else if(GameManager.Instance.HasValidLoopshapes) {
				GrabbingObject = null;
				foreach(var loopshape in GameManager.Instance.ValidLoopshapes)
					loopshape.Open();
			}
			#endregion
		}
	}
}