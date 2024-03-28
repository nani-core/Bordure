using UnityEngine;

namespace NaniCore.Bordure {
	public class WaterPump : Waterlet {
		#region Serialized fields
		[SerializeField] private WaterStream waterStreamTemplate;
		[Min(0)] public float waterStreamRadius = 1.0f;
		#endregion

		#region Fields
		[System.NonSerialized] public WaterStream lastWaterStream = null;
		#endregion

		#region Interfaces
		public override bool IsSatisfied => water.Height >= Height;

		public Vector3 EjectionVelocity => GameManager.Instance.Settings.defaultWaterEjectionVelocity;

		public Vector3 PositionInTime(float time) {
			return (EjectionVelocity + Physics.gravity * (time * .5f)) * time;
		}

		public float TimeInHeight(float height) {
			float g = Physics.gravity.magnitude;
			float vy = -EjectionVelocity.y;
			return (Mathf.Sqrt(vy * vy + g * height * 2.0f) - vy) / g;
		}

		public float WaterTouchingTime => TimeInHeight(transform.position.y - Water.WorldHeight);
		#endregion

		#region Message handler
		public void OnWaterStreamTouchedWater(WaterStream stream) {
			if(stream != lastWaterStream || Water.HasAnyActiveWaterletsOtherThan(this))
				return;

			Water.TargetHeight = Height;
		}

		public void OnWaterStreamFullyEnteredWater(WaterStream stream) {
			if(stream != lastWaterStream || Water.HasAnyActiveWaterletsOtherThan(this))
				return;

			Water.TargetHeight = Water.Height;
		}
		#endregion

		#region Life cycle
		protected new void OnEnable() {
			base.OnEnable();

			if(IsSatisfied)
				return;

			Water.OnWaterletEnabled(this);

			// Generate new water stream.
			var waterStream = Instantiate(waterStreamTemplate.gameObject, pivot).GetComponent<WaterStream>();
			waterStream.pump = this;
			lastWaterStream = waterStream;
		}

		protected void OnDisable() {
			if(lastWaterStream != null) {
				lastWaterStream.isFlowing = false;
			}
		}
		#endregion
	}
}