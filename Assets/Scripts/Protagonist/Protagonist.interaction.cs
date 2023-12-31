using UnityEngine;
using System.Collections;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private FocusUi focus;
		#endregion

		#region Fields
		private Interactable focusingObject;
		private Grabbable grabbingObject;
		private bool grabbingOrienting;
		private LoopShape satisfiedLoopShape;
		#endregion

		#region Interfaces
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

				UpdateFocusUi();
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

				UpdateFocusUi();
				inputHandler.SetGrabbingActionEnabled(grabbingObject != null);
			}
		}

		public bool GrabbingOrienting {
			get => grabbingObject != null && grabbingOrienting;
			set => grabbingOrienting = grabbingObject != null && value;
		}

		public LoopShape SatisfiedLoopShape {
			get => satisfiedLoopShape;
			set {
				if(satisfiedLoopShape == value)
					return;

				if(satisfiedLoopShape)
					satisfiedLoopShape.SendMessage("OnLoopShapeUnsatisfy", SendMessageOptions.DontRequireReceiver);
				satisfiedLoopShape = value;
				if(satisfiedLoopShape)
					satisfiedLoopShape.SendMessage("OnLoopShapeSatisfy", SendMessageOptions.DontRequireReceiver);

				UpdateFocusUi();
			}
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
		#endregion

		#region Life cycle
		private void InitializeInteraction() {
			inputHandler = gameObject.EnsureComponent<ProtagonistInputHandler>();

			if(focus == null) {
				Debug.LogWarning("No FocusUi component found in the protagonist interaction UI prefab.", this);
			}

			FocusingObject = null;
		}

		private void LateUpdateInteraction() {
			bool hasHit = Raycast(out RaycastHit hit);

			// If not grabbing anything, check for focus.
			if(GrabbingObject == null) {
				if(!hasHit)
					FocusingObject = null;
				else {
					// Don't focus on inactive targets.
					bool set = false;
					foreach(var interactable in hit.transform.GetComponents<Interactable>()) {
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
				if(hasHit) {
					bool isHitPointIntertweening = Vector3.Distance(hit.point, eye.position) < Vector3.Distance(GrabbingObject.transform.position, eye.position);
					bool isNotDescendantOfGrabbingObject = !hit.transform.IsChildOf(GrabbingObject.transform);
					if(isHitPointIntertweening && isNotDescendantOfGrabbingObject)
						GrabbingObject = null;
				}
			}
			// If any loop shape is satisfied, check for invalidation.
			if(SatisfiedLoopShape != null) {
				if(!SatisfiedLoopShape.isActiveAndEnabled || !SatisfiedLoopShape.Validate(eye))
					SatisfiedLoopShape = null;
			}
			// If no loop shape is satisfied, seek for activation.
			else {
				foreach(var loopshape in LoopShape.All) {
					if(!loopshape.isActiveAndEnabled)
						continue;
					if(loopshape.Validate(eye)) {
						SatisfiedLoopShape = loopshape;
						break;
					}
				}
			}
		}
		#endregion

		#region Functions
		private void UpdateFocusUi() {
			if(focus == null)
				return;
			if(SatisfiedLoopShape)
				focus.UpdateFocusAnimated(1);
			else if(GrabbingObject)
				focus.UpdateFocusAnimated(2);
			else if(FocusingObject)
				focus.UpdateFocusAnimated(1);
			else
				focus.UpdateFocusAnimated(0);
		}

		private bool Raycast(out RaycastHit hitInfo) {
			var ray = Camera.ViewportPointToRay(Vector2.one * .5f);
			return PhysicsUtility.Raycast(ray.origin, ray.direction, out hitInfo, Profile.maxInteractionDistance, GameManager.Instance.GrabbingLayerMask, false);
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
				target.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
				target.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
				yield return new WaitForFixedUpdate();
			}
			target.transform.localPosition = endPosition;
			target.transform.localRotation = endRotation;
		}

		public void Interact() {
			if(SatisfiedLoopShape != null) {
				GrabbingObject = null;
				SatisfiedLoopShape.SendMessage("OnLoopShapeOpen");
				SatisfiedLoopShape = null;
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