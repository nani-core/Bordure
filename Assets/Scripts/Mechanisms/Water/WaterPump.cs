using UnityEngine;

namespace NaniCore.Stencil {
	public class WaterPump : Waterlet {
		#region Serialized fields
		[SerializeField] private Mesh waterStreamBaseMesh;
		[SerializeField][Min(0)] private float waterStreamRadius = 1.0f;
		#endregion

		#region Fields
		private MeshRenderer waterStreamRenderer;
		private Material waterStreamMat;
		private float motionTime = 0.0f;
		/// <summary>
		/// How long should the water stream fall additionally to prevent a visual glitch.
		/// </summary>
		/// <remarks>
		/// TODO: The calculation for this field should be optimized in the future.
		/// </remarks>
		private float waterStreamAdditionalHeightGain = 0.0f;
		#endregion

		#region Interfaces
		public override bool IsSatisfied => water.Height >= Height;
		#endregion

		#region Functions
		protected new void Start() {
			base.Start();

			var waterStreamObj = new GameObject("Water Stream");
			waterStreamObj.transform.SetParent(pivot, false);
			waterStreamRenderer = waterStreamObj.AddComponent<MeshRenderer>();
			waterStreamObj.AddComponent<MeshFilter>().sharedMesh = waterStreamBaseMesh;

			waterStreamMat = new Material(GameManager.Instance.Settings.waterStreamMaterial);
			waterStreamRenderer.sharedMaterial = waterStreamMat;
			waterStreamMat.SetVector("_Water_Radius", Vector2.one * waterStreamRadius);
			waterStreamMat.SetVector("_Gravity", Physics.gravity);
			waterStreamMat.SetVector("_Initial_Velocity", GameManager.Instance.Settings.defaultWaterEjectionVelocity);

			waterStreamAdditionalHeightGain = 1.0f;

			UpdateVisualState();
		}

		protected void FixedUpdate() {
			if(motionTime < 100.0f) {
				motionTime += Time.fixedDeltaTime;
			}
		}

		protected override void UpdateVisualState() {
			if(waterStreamRenderer != null) {
				waterStreamRenderer.gameObject.SetActive(enabled);
				motionTime = 0.0f;
			}
		}

		protected override void UpdateVisualFrame() {
			if(waterStreamRenderer != null) {
				float deltaHeight = Height - water.Height;
				waterStreamMat.SetFloat("_Water_Height", deltaHeight + waterStreamAdditionalHeightGain);
				float startTime, endTime;

				if(enabled)
					(startTime, endTime) = (0.0f, motionTime);
				else
					(startTime, endTime) = (motionTime, 100.0f);

				waterStreamMat.SetFloat("_Start_Time", startTime);
				waterStreamMat.SetFloat("_End_Time", endTime);
			}
		}
		#endregion
	}
}