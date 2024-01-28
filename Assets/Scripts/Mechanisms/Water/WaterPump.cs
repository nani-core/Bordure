using UnityEngine;

namespace NaniCore.Stencil {
	public class WaterPump : Waterlet {
		#region Serialized fields
		[SerializeField] private ParticleSystem particle;
		#endregion

		#region Interfaces
		public override bool IsSatisfied => water.Height >= Height;
		#endregion

		#region Functions
		protected override void UpdateVisualState() {
			if(particle != null) {
				var particleEmmision = particle.emission;
				particleEmmision.enabled = enabled;
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