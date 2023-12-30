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
		private Coroutine grabbingCoroutine;
		private bool grabbingOrienting;
		private LoopShape satisfiedLoopShape;
		#endregion

		#region Properties
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
					PlaySfx(profile.onFocusSound);
				}

				UpdateFocusUi();
			}
		}

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
		#endregion

		#region Life cycle
		private void StartInteraction() {
			inputHandler = gameObject.EnsureComponent<ProtagonistInputHandler>();

			if(focus == null) {
				Debug.LogWarning("No FocusUi component found in the protagonist interaction UI prefab.", this);
			}

			FocusingObject = null;
		}

		private void LateUpdateInteraction() {
			// If not grabbing anything, check for focus.
			if(GrabbingObject == null) {
				bool isHit = Raycast(out RaycastHit hitInfo);
				if(!isHit)
					FocusingObject = null;
				else {
					// Don't focus on inactive targets.
					bool set = false;
					foreach(var interactable in hitInfo.transform.GetComponents<Interactable>()) {
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
				bool isHit = Raycast(out RaycastHit hitInfo);
				// Don't drop if not hit, might be due to orienting too fast.
				if(isHit) {
					bool isHitPointIntertweening = Vector3.Distance(hitInfo.point, eye.position) < Vector3.Distance(GrabbingObject.transform.position, eye.position);
					bool isNotDescendantOfGrabbingObject = !hitInfo.transform.IsChildOf(GrabbingObject.transform);
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
			return Physics.Raycast(Camera.ViewportPointToRay(Vector2.one * .5f), out hitInfo, profile.maxInteractionDistance, GameManager.Instance.GrabbingLayerMask);
		}

		#region Grabbing
		public void GrabbingOrientDelta(float delta) {
			delta *= profile.orientingSpeed;
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
			PlaySfx(profile.onGrabSound);

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
			for(float t; (t = (Time.time - startTime) / profile.grabbingTransitionDuration) < 1;) {
				t = MathUtility.Ease(t, profile.grabbingEasingFactor);
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
			PlaySfx(profile.onDropSound);

			yield break;
		}
		#endregion

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