using UnityEngine;

namespace NaniCore.Bordure {
	public class WaterDump : Waterlet {
		#region Serialized fields
		[SerializeField] private Renderer swirl;
		#endregion

		#region Interfaces
		public override bool IsSatisfied => water.Height <= Height;
		#endregion

		#region Functions
		public override void OnWaterHeightChange(float previousHeight) {
			base.OnWaterHeightChange(previousHeight);
			UpdateVisualFrame();
		}

		private void UpdateVisualState() {
			swirl.enabled = enabled;
		}

		private void UpdateVisualFrame() {
			float angle = Time.fixedDeltaTime * 360 * 2;
			swirl.transform.rotation *= Quaternion.Euler(Vector3.forward * angle);
		}
		#endregion

		#region Life cycle
		protected new void OnEnable() {
			base.OnEnable();

			if(IsSatisfied)
				return;

			UpdateVisualState();
			Water.OnWaterletEnabled(this);
			Water.TargetHeight = Height;
		}

		protected void OnDisable() {
			UpdateVisualState();
			Water.TargetHeight = Water.Height;
		}
		#endregion
	}
}