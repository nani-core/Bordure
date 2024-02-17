using UnityEngine;

namespace NaniCore.Stencil {
	public class WaterDump : Waterlet {
		#region Serialized fields
		[SerializeField] private Renderer swirl;
		#endregion

		#region Interfaces
		public override bool IsSatisfied => water.Height <= Height;
		#endregion

		#region Functions
		protected override void UpdateVisualState() {
			if(swirl) {
				swirl.enabled = enabled;
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