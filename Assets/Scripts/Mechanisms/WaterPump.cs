using UnityEngine;

namespace NaniCore.UnityPlayground {
	public class WaterPump : Waterlet {
		#region Serialized fields
		[SerializeField] private ParticleSystem particle;
		#endregion

		#region Fields
		bool isFlowing;
		#endregion

		#region Message handlers
		protected override void OnSetActivity() {
			water.UpdateTargetHeight();
			UpdateFlowingState();
		}

		protected override void OnWaterLevelPass() {
			UpdateFlowingState();
		}
		#endregion

		#region Functions
		public bool IsFlowing {
			get => isFlowing;
			set {
				isFlowing = value;
				if(particle != null) {
					var particleEmmision = particle.emission;
					particleEmmision.enabled = value;
				}
			}
		}

		private void UpdateFlowingState() {
			IsFlowing = IsActive && water != null && water.Height < Height;
		}
		#endregion

		#region Life cycle
		protected void FixedUpdate() {
			if(isFlowing) {
				if(particle != null) {
					var main = particle.main;
					var speed = main.startSpeed.constant;
					main.startLifetime = (Height - water.Height) / speed;
				}
			}
		}
		#endregion
	}
}