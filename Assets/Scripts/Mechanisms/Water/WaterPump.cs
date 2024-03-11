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

			waterStreamMat = new Material(GameManager.Instance.Settings.waterStreamShader);
			waterStreamRenderer.sharedMaterial = waterStreamMat;
			waterStreamMat.SetVector("_Water_Radius", Vector2.one * waterStreamRadius);
			waterStreamMat.SetVector("_Gravity", Physics.gravity);
			waterStreamMat.SetVector("_Initial_Velocity", GameManager.Instance.Settings.defaultWaterEjectionVelocity);

			UpdateVisualState();
		}

		protected override void UpdateVisualState() {
			if(waterStreamRenderer != null) {
				waterStreamRenderer.gameObject.SetActive(enabled);
			}
		}

		protected override void UpdateVisualFrame() {
			if(waterStreamRenderer != null) {
				float deltaHeight = Height - water.Height;
				waterStreamMat.SetFloat("_Water_Height", deltaHeight);
			}
		}
		#endregion
	}
}