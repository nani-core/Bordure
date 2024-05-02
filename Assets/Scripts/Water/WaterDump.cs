using UnityEngine;

namespace NaniCore.Bordure {
	public class WaterDump : Waterlet {
		#region Serialized fields
		[SerializeField] private Renderer swirl;
		[SerializeField] private ParticleSystem particles;
		#endregion

		#region Interfaces
		public override bool IsSatisfied => water.Height <= Height;
		#endregion

		#region Functions
		public override void OnWaterHeightChange(float previousHeight) {
			base.OnWaterHeightChange(previousHeight);
			UpdateVisualFrame();
		}

		private void UpdateVisualState()
		{
			if (particles != null)
			{
				if(enabled)
					particles.Play();
				else
					particles.Stop();
			}
			//swirl.enabled = enabled;
		}

		private void UpdateVisualFrame() {
			//float angle = Time.fixedDeltaTime * 360 * 2;
			//swirl.transform.rotation *= Quaternion.Euler(Vector3.forward * angle);
		}

		protected override void Activate() {
			UpdateVisualState();
			Water.OnWaterletEnabled(this);
			Water.TargetHeight = Height;
		}

		protected override void Deactivate() {
			UpdateVisualState();
			Water.TargetHeight = Water.Height;
		}
		#endregion
	}
}