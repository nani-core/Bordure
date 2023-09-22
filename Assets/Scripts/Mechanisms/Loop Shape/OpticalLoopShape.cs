using UnityEngine;
using UnityEngine.Rendering;

namespace NaniCore.Loopool {
	public class OpticalLoopShape : LoopShape {
		#region Serialized fields
		/// »·
		[SerializeField] private GameObject blasto;
		/// µº
		[SerializeField] private GameObject gastro;
		#endregion

		#region Fields
		private Mrt blastoMrt, gastroMrt;
		private RenderTexture mrtTexture;
		private bool validated = false;
		#endregion

		#region Functions
		public override bool Validate(Transform eye) => validated;

		private void PerformValidation(RenderTexture cameraOutput) {
			if(blastoMrt == null || gastroMrt == null)
				return;

			RenderUtility.SetValue(mrtTexture, Color.black);

			blastoMrt.RenderToTexture(mrtTexture);
			gastroMrt.RenderToTexture(mrtTexture);

			RenderUtility.ReplaceByValue(mrtTexture, Color.black, cameraOutput);
		}
		#endregion

		#region Message handlers
		private void OnPostFrameRender(Camera camera, RenderTexture cameraOutput) {
			PerformValidation(cameraOutput);
			Graphics.Blit(mrtTexture, cameraOutput);
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			base.Start();

			blastoMrt = blasto.GetComponent<Mrt>() ?? blasto.AddComponent<Mrt>();
			gastroMrt = gastro.GetComponent<Mrt>() ?? gastro.AddComponent<Mrt>();
		}

		protected new void OnDestroy() {
			base.OnDestroy();

			if(blastoMrt)
				Destroy(blastoMrt);
			if(gastroMrt)
				Destroy(gastroMrt);
		}

		protected void OnEnable() {
			mrtTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
		}

		protected void OnDisable() {
			RenderTexture.ReleaseTemporary(mrtTexture);
			mrtTexture = null;
		}

		protected void OnBecameVisible() {
			if(MainCamera.Instance)
				MainCamera.Instance.onPostFrameRender += OnPostFrameRender;
		}

		protected void OnBecameInvisible() {
			if(MainCamera.Instance)
				MainCamera.Instance.onPostFrameRender -= OnPostFrameRender;
			validated = false;
		}
		#endregion
	}
}