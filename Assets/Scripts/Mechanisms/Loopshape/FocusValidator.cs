using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Stencil {
	public class FocusValidator : LoopshapeValidator {
		#region Serialized fields
		[SerializeField] private bool overrideTarget;
		[ShowIf("overrideTarget")][SerializeField] private GameObject target;
		[SerializeField] private bool includeChildren = true;
		[SerializeField][Min(0f)] private float maxDistance = 0f;
		#endregion

		#region Functions
		public GameObject Target {
			get => overrideTarget ? target ?? gameObject : gameObject;
			set {
				if(value == null)
					value = gameObject;
				overrideTarget = value == gameObject;
				if(overrideTarget)
					target = value;
			}
		}

		public float MaxDistance => maxDistance == 0f ? GameManager.Instance.Protagonist.Profile.maxInteractionDistance : maxDistance;

		protected override bool Validate() {
			if(!isActiveAndEnabled)
				return false;

			var protagonist = GameManager.Instance?.Protagonist;
			if(protagonist == null)
				return false;

			if(protagonist.LookingAtObject == null)
				return false;
			if(includeChildren) {
				if(!protagonist.LookingAtObject.IsChildOf(Target))
					return false;
			}
			else {
				if(protagonist.LookingAtObject != Target)
					return false;
			}
			if(Vector3.Distance(protagonist.Eye.position, protagonist.LookingPosition) > MaxDistance)
				return false;
			return true;
		}
		#endregion
	}
}