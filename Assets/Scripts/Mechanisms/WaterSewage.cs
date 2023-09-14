using UnityEngine;

namespace NaniCore.UnityPlayground {
	public class WaterSewage : Waterlet {
		#region Serialized fields
		[SerializeField] private Renderer swirl;
		#endregion

		#region Functions
		protected override void UpdateFlowingState() {
			IsFlowing = IsActive && water != null && water.Height > Height;
		}

		protected override void UpdateVisualState() {
			if(swirl) {
				swirl.enabled = IsFlowing;
			}
		}

		protected override void UpdateVisualFrame() {
			if(swirl) {
				swirl.transform.rotation *= Quaternion.Euler(0, Time.fixedDeltaTime * 360 * 2, 0);
			}
		}
		#endregion
	}
}