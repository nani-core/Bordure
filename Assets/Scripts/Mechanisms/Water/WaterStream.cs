using UnityEngine;

namespace NaniCore.Stencil {
	public class WaterStream : MonoBehaviour {
		#region Serialized field
		[SerializeField] new MeshRenderer renderer;
		#endregion

		#region Fields
		[System.NonSerialized] public WaterPump pump;

		Material material;

		float startTime = 0.0f, endTime = 0.0f;
		/// <summary>
		/// Whether the pump is pumping out water.
		/// </summary>
		[System.NonSerialized] public bool isFlowing = true;
		/// <summary>
		/// Whether the bottom end of the stream has touched the water surface.
		/// </summary>
		[System.NonSerialized] public bool hasTouchedWater = false;
		#endregion

		#region Functions
		private void UpdateState(float deltaTime) {
			if(!isFlowing)
				startTime += deltaTime;
			if(!hasTouchedWater)
				endTime += deltaTime;

			if(!hasTouchedWater && endTime >= pump.WaterTouchingTime) {
				hasTouchedWater = true;
				endTime = pump.WaterTouchingTime;
				pump.OnWaterStreamTouchedWater(this);
			}
		}

		private void UpdateVisual() {
			transform.localPosition = pump.PositionInTime(startTime);
			material.SetVector("_Initial_Velocity", pump.EjectionVelocity + Physics.gravity * startTime);
			material.SetFloat("_Duration", endTime - startTime);
		}
		#endregion

		#region Life cycle
		public void Start() {
			if(pump == null) {
				Destroy(gameObject);
				return;
			}

			// Automatically create a runtime copy of the original material.
			material = renderer.material;

			material.SetVector("_Water_Radius", Vector2.one * pump.waterStreamRadius);
			material.SetVector("_Gravity", Physics.gravity);

			UpdateVisual();
		}

		public void FixedUpdate() {
			UpdateState((float)Time.fixedDeltaTime);

			if(startTime > endTime) {
				pump.OnWaterStreamFullyEnteredWater(this);
				Destroy(gameObject);
				return;
			}

			UpdateVisual();
		}

		protected void OnDestroy() {
			if(pump.lastWaterStream == this)
				pump.lastWaterStream = null;
		}
		#endregion
	}
}
