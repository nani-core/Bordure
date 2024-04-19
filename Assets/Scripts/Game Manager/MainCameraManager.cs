using UnityEngine;
using UnityEngine.Rendering;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Camera))]
	public class MainCameraManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private UnityEngine.UI.RawImage renderOutputImage;
		#endregion

		#region Fields
		private new Camera camera;
		private RenderTexture renderOutputTexture;
		#endregion

		#region Properties
		public Camera Camera => camera;
		#endregion

		#region Life cycle
		protected void Start() {
			camera = GetComponent<Camera>();
			SetupAndUpdateRenderOutputTexture();
			RenderPipelineManager.beginCameraRendering += OnSRPCameraPreRender;
			RenderPipelineManager.endCameraRendering += OnSRPCameraPostRender;
		}

		protected void OnPreRender() {
			SetupAndUpdateRenderOutputTexture();
		}

		protected void OnPostRender() {
			//
		}
		#endregion

		#region SRP event handlers
		private void OnSRPCameraPreRender(ScriptableRenderContext context, Camera camera) {
			if(camera != Camera)
				return;
			OnPreRender();
		}

		private void OnSRPCameraPostRender(ScriptableRenderContext context, Camera camera) {
			if(camera != Camera)
				return;
			OnPostRender();
		}
		#endregion

		#region Functions
		private void SetupAndUpdateRenderOutputTexture() {
			bool needsRedirecting = false;
			if(renderOutputTexture == null) {
				renderOutputTexture = RenderUtility.CreateScreenSizedRT(RenderTextureFormat.Default);
				needsRedirecting = true;
			}
			else if(RenderUtility.Resize(ref renderOutputTexture, RenderUtility.ScreenSize))
				needsRedirecting = true;
			if(needsRedirecting) {
				camera.targetTexture = renderOutputTexture;
				renderOutputImage.texture = renderOutputTexture;
			}
		}
		#endregion
	}
}
