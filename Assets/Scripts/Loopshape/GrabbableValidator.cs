using UnityEngine;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Grabbable))]
	public class GrabbableValidator : FocusValidator {
		#region Fields
		private Grabbable grabbable;
		#endregion

		#region Functions
		protected Grabbable Grabbable {
			get {
				if(grabbable == null)
					grabbable = GetComponent<Grabbable>();

				return grabbable;
			}
		}
		
		protected override bool Validate() {
			if(!isActiveAndEnabled)
				return false;

			// This kind of validator *is* validated when the player is only
			// focusing on but not acutally grabbing the target object.
			if(!Grabbable.IsGrabbed)
				return base.Validate();
			else
				return false;
		}

		private bool IsGrabbingOccluded() {
			if(!Grabbable.IsGrabbed)
				return false;
			var protagonist = GameManager.Instance.Protagonist;
			Vector3 origin = protagonist.Eye.position, position = Target.transform.position;
			Ray ray = new(origin, position - origin);
			bool hasHit = PhysicsUtility.Raycast(ray, out RaycastHit hitInfo, protagonist.Profile.maxInteractionDistance, GameManager.Instance.InteractionLayerMask, false);
			bool isNotOccluded = !hasHit
				// Often times the hit will lag a little.
				|| Vector3.Distance(origin, hitInfo.point) >= Vector3.Distance(origin, position)
				|| hitInfo.transform.IsChildOf(Target.transform);
			return !isNotOccluded;
		}
		#endregion

		#region Life cycle
		protected new void Update() {
			base.Update();

			if(IsGrabbingOccluded())
				Grabbable.Drop();
		}
		#endregion

		#region Message handlers
		// When stepped by protagonist.
		protected void OnStepped() {
			enabled = false;
		}

		// When not stepped any more by protagonist.
		protected void OnNotStepped() {
			enabled = true;
		}
		#endregion
	}
}