using UnityEngine;

namespace NaniCore.Loopool {
	public class WaterPump : Waterlet {
		#region Serialized fields
		[SerializeField] private ParticleSystem particle;
		#endregion

		#region Functions
		protected override void UpdateFlowingState() {
			IsFlowing = IsActive && water != null && water.Height < Height;
		}

		protected override void UpdateVisualState() {
			if(particle != null) {
				var particleEmmision = particle.emission;
				particleEmmision.enabled = IsFlowing;
			}
		}

		protected override void UpdateVisualFrame() {
			if(particle != null) {
				var main = particle.main;
				var speed = main.startSpeed.constant;
				main.startLifetime = (Height - water.Height) / speed;
			}
		}
		#endregion
	}
}