using UnityEngine;
using System.Collections;

namespace NaniCore.Bordure {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private FocusUi focus;
		#endregion

		#region Fields
		private GameObject lookingAtObject = null;
		private Transform grabbingObject;
		private bool grabbingOrienting;
		private RaycastHit lookingHit;
		#endregion

		#region Interfaces
		public GameObject LookingAtObject => lookingAtObject;
		public Vector3 LookingPosition => lookingHit.point;

		public Transform GrabbingObject {
			get => grabbingObject;
			set {
				if(grabbingObject == value)
					return;

				if(grabbingObject != null) {
					grabbingObject.transform.SetParent(null, true);
					grabbingObject.SendMessage("OnGrabEnd");
					PlaySfx(GameManager.Instance.Settings.audio.onDropSound);
					Debug.Log($"{grabbingObject} is dropped.", grabbingObject);
				}

				grabbingObject = value;

				if(grabbingObject != null) {
					grabbingObject.SendMessage("OnGrabBegin");
					PlaySfx(GameManager.Instance.Settings.audio.onGrabSound);

					grabbingObject.transform.SetParent(eye.transform, true);
					Debug.Log($"{grabbingObject} is grabbed.", grabbingObject);
				}

				InputHandler.UsesGrabbing = grabbingObject != null;
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
		private void UpdateInteraction() {
			bool hasHit = EyeCast(out lookingHit);

			GameObject newLookingAtObject = hasHit ? lookingHit.transform.gameObject : null;
			if(newLookingAtObject != lookingAtObject) {
				lookingAtObject = newLookingAtObject;
				//Debug.Log($"Now looking at {newLookingAtObject}.", newLookingAtObject);
			}

			UpdateFocusUi();
		}
		#endregion

		#region Functions
		private void UpdateFocusUi() {
			if(focus == null)
				return;

			float effectiveCastDistance = Vector3.Distance(lookingHit.point, Eye.position);
			if(GameManager.Instance.HasValidLoopshapes) {
				focus.CurrentStatus = FocusUi.Status.Hovering;
				float maxCastDistance = Profile.maxInteractionDistance;
				foreach(var loopshape in GameManager.Instance.ValidLoopshapes) {
					foreach(var validator in loopshape.ValidValidators) {
						if(validator is not FocusValidator)
							continue;
						var fv = validator as FocusValidator;
						maxCastDistance = Mathf.Min(maxCastDistance, fv.MaxDistance);
					}
				}
				// focus.Opacity = 1 - Mathf.Clamp01(effectiveCastDistance / maxCastDistance);
			}
			else if(!GrabbingObject) {
				focus.CurrentStatus = FocusUi.Status.Normal;
				// focus.Opacity = 1 - Mathf.Clamp01(effectiveCastDistance / Profile.maxInteractionDistance);
			}
			else {
				focus.CurrentStatus = FocusUi.Status.Grabbing;
				// focus.Opacity = 1f;
			}
			// focus.Opacity = 1f;
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
			if(GameManager.Instance.HasValidLoopshapes) {
				GrabbingObject = null;
				foreach(var loopshape in GameManager.Instance.ValidLoopshapes)
					loopshape.Open();
			}
			#endregion
		}
	}
}