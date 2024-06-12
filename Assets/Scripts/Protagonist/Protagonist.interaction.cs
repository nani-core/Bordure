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

				// Cannot grab the object being stood on.
				if(value != null && IsOnGround) {
					if(steppingGround.transform?.IsChildOf(value) ?? false) {
						Debug.LogWarning($"Warning: Cannot grab {value} now because it is being stood on.", value);
						return;
					}
				}
				grabbingObject = value;

				if(grabbingObject != null) {
					grabbingObject.SendMessage("OnGrabBegin");
					PlaySfx(GameManager.Instance.Settings.audio.onGrabSound);

					grabbingObject.transform.SetParent(eye.transform, true);
					Debug.Log($"{grabbingObject} is grabbed.", grabbingObject);

					GrabbingDistance -= 0.04f; // To prevent clipping through floor.
				}
			}
		}

		public void GrabbingOrientDelta(Vector3 delta) {
			delta *= Profile.orientingSpeed;
			Vector3 euler = new Vector3(delta.y, -delta.x, 0) * Mathf.Rad2Deg;
			Quaternion deltaQuat = Quaternion.Euler(euler);
			Quaternion t = Quaternion.Inverse(GrabbingObject.rotation) * Eye.rotation;
			deltaQuat = t * deltaQuat * Quaternion.Inverse(t);
			GrabbingObject.localRotation *= deltaQuat;
		}

		public float GrabbingDistance {
			get {
				if(GrabbingObject == null)
					return Mathf.Infinity;
				return Vector3.Distance(Eye.position, GrabbingObject.position);
			}
			set {
				if(GrabbingObject == null)
					return;

				float min = 0;
				if(GrabbingObject.TryGetComponent(out Collider shape))
					min = shape.bounds.size.magnitude;
				float max = Profile.maxInteractionDistance;
				if(GrabbingObject.TryGetComponent(out FocusValidator focusValidator))
					max = Mathf.Min(max, focusValidator.MaxDistance);
				value = Mathf.Clamp(value, min, max);

				Vector3 direction = Vector3.Normalize(GrabbingObject.position - Eye.position);
				GrabbingObject.position = Eye.position + direction * value;
			}
		}

		public bool EyeCast(out RaycastHit hit) {
			return PhysicsUtility.Raycast(EyeRay, out hit, Profile.maxInteractionDistance, GameManager.Instance.InteractionLayerMask, false);
		}
		#endregion

		#region Life cycle
		private void UpdateInteraction() {
			if(GrabbingObject == null) {
				bool hasHit = EyeCast(out lookingHit);

				GameObject newLookingAtObject = hasHit ? lookingHit.transform.gameObject : null;
				if(newLookingAtObject != lookingAtObject) {
					lookingAtObject = newLookingAtObject;
					//Debug.Log($"Now looking at {newLookingAtObject}.", newLookingAtObject);
				}
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