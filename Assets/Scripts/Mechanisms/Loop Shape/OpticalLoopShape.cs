using UnityEngine;
using UnityEngine.Rendering;

namespace NaniCore.Loopool {
	public class OpticalLoopShape : LoopShape {
		private const string mrtShaderName = "NaniCore/MRT";
		private static Shader mrtShader;

		#region Serialized fields
		[SerializeField] private Renderer displayRenderer;
		[SerializeField] private Color mrtValue = Color.white;
		#endregion

		#region Fields
		private RenderTexture mrtTexture;
		private Material mrtMaterial;
		#endregion

		#region Functions
		public override bool Satisfied(Transform eye) => false;

		#endregion

		#region Life cycle
		protected void Awake() {
			mrtShader = Shader.Find(mrtShaderName);
		}

		protected void OnEnable() {
			mrtMaterial = new Material(mrtShader);
			mrtTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
			displayRenderer.material.mainTexture = mrtTexture;
			if(MainCamera.Instance)
				MainCamera.Instance.onPostFrameRender += RenderMrt;
		}

		protected void OnDisable() {
			RenderTexture.ReleaseTemporary(mrtTexture);
			mrtTexture = null;
			if(MainCamera.Instance)
				MainCamera.Instance.onPostFrameRender -= RenderMrt;
		}

		public void RenderMrt(Camera camera, RenderTexture cameraOutput) {
			RenderUtility.SetValue(mrtTexture, Color.black);

			var cb = new CommandBuffer();
			cb.CopyTexture(cameraOutput, mrtTexture);
			Graphics.ExecuteCommandBuffer(cb);

			mrtMaterial.SetColor("_Value", mrtValue);
			RenderUtility.RenderObject(mrtTexture.colorBuffer, cameraOutput.depthBuffer, gameObject, camera, mrtMaterial);

			Graphics.Blit(mrtTexture, cameraOutput);
		}
		#endregion
	}
}