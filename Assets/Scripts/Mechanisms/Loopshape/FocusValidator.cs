using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Stencil {
	public class FocusValidator : LoopshapeValidator {
		#region Serialized fields
		[SerializeField] private bool overrideTarget;
		[ShowIf("overrideTarget")][SerializeField] private GameObject target;
		[SerializeField] public bool includeChildren = true;
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

		protected override bool Validate() {
			if(!isActiveAndEnabled)
				return false;

			var protagonist = GameManager.Instance?.Protagonist;
			if(protagonist == null)
				return false;

			if(protagonist.LookingAtObject == null)
				return false;
			return protagonist.LookingAtObject.IsChildOf(Target);
		}
		#endregion
	}
}