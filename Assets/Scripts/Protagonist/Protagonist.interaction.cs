using UnityEngine;
using System.Collections;

namespace NaniCore.Stencil {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private FocusUi focus;
		#endregion

		#region Fields
		private GameObject lookingAtObject = null;
		private Interactable focusingObject;
		private Grabbable grabbingObject;
		private RaycastHit lookingHit;
		private bool grabbingOrienting;
		#endregion

		#region Interfaces
		public GameObject LookingAtObject => lookingAtObject;

		public Interactable FocusingObject {
			get => focusingObject;
			set {
				if(focusingObject == value)
					return;

				if(focusingObject)
					focusingObject.SendMessage("OnFocusLeave", SendMessageOptions.DontRequireReceiver);
				focusingObject = value;
				if(focusingObject) {
					focusingObject.SendMessage("OnFocusEnter", SendMessageOptions.DontRequireReceiver);
					PlaySfx(Profile.onFocusSound);
				}
			}
		}

		public Grabbable GrabbingObject {
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
			get => grabbingObject != null && grabbingOrienting;
			set => grabbingOrienting = grabbingObject != null && value;
		}

		public void GrabbingOrientDelta(float delta) {
			delta *= Profile.orientingSpeed;
			float grabbingAzimuth = grabbingObject.transform.localRotation.eulerAngles.y * Mathf.PI / 180;
			grabbingAzimuth += delta;
			grabbingObject.transform.localRotation = Quaternion.Euler(0, grabbingAzimuth * 180 / Mathf.PI, 0);
		}

		public void ResetGrabbingTransform() {
			if(GrabbingObject == null)
				return;

			StartCoroutine(GrabCoroutine(grabbingObject));
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

			FocusingObject = null;
		}

		private void UpdateInteraction() {
			bool hasHit = EyeCast(out RaycastHit lookingHit);
			lookingAtObject = hasHit ? lookingHit.transform.gameObject : null;

			UpdateFocusUi();
		}

		private void LateUpdateInteraction() {
			// If not grabbing anything, check for focus.
			if(GrabbingObject == null) {
				if(LookingAtObject == null)
					FocusingObject = null;
				else {
					// Don't focus on inactive targets.
					bool set = false;
					foreach(var interactable in LookingAtObject.transform.GetComponents<Interactable>()) {
						if(!interactable.isActiveAndEnabled)
							continue;
						FocusingObject = interactable;
						set = true;
					}
					if(!set)
						FocusingObject = null;
				}
			}
			// If grabbing blocked, drop.
			else {
				// Don't drop if not hit, might be due to orienting too fast.
				if(LookingAtObject != null) {
					bool isHitPointIntertweening = Vector3.Distance(lookingHit.point, eye.position) < Vector3.Distance(GrabbingObject.transform.position, eye.position);
					bool isNotDescendantOfGrabbingObject = !LookingAtObject.IsChildOf(GrabbingObject.transform);
					if(isHitPointIntertweening && isNotDescendantOfGrabbingObject)
						GrabbingObject = null;
				}
			}
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
			else if(FocusingObject)
				focus.CurrentStatus = FocusUi.Status.Hovering;
			else
				focus.CurrentStatus = FocusUi.Status.Normal;
		}

		private IEnumerator GrabCoroutine(Grabbable target) {
			float grabbingDistance = Vector3.Distance(target.transform.position, eye.transform.position);
			float grabbingAzimuth = target.transform.localRotation.eulerAngles.y * Mathf.PI / 180;

			Vector3
				startPosition = target.transform.localPosition,
				endPosition = Vector3.forward * grabbingDistance;
			Quaternion
				startRotation = target.transform.localRotation,
				endRotation = Quaternion.Euler(0, grabbingAzimuth * 180 / Mathf.PI, 0);

			float startTime = Time.time;
			for(float t; (t = (Time.time - startTime) / Profile.grabbingTransitionDuration) < 1;) {
				t = MathUtility.Ease(t, Profile.grabbingEasingFactor);
				target.transform.SetLocalPositionAndRotation(
					Vector3.Lerp(startPosition, endPosition, t),
					Quaternion.Slerp(startRotation, endRotation, t)
				);
				yield return new WaitForFixedUpdate();
			}
			target.transform.localPosition = endPosition;
			target.transform.localRotation = endRotation;
		}

		public void Interact() {
			if(GameManager.Instance.HasValidLoopshapes) {
				GrabbingObject = null;
				foreach(var loopshape in GameManager.Instance.ValidLoopshapes)
					loopshape.Open();
			}
			else if(GrabbingObject != null) {
				GrabbingObject = null;
			}
			else if(FocusingObject != null) {
				FocusingObject.SendMessage("OnInteract");
			}
		}
		#endregion
	}
}