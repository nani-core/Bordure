using UnityEngine;
using UnityEngine.Rendering;

namespace NaniCore {
	[RequireComponent(typeof(Camera))]
	public class CameraRenderingTarget : MonoBehaviour {
		#region Fields
		private bool initialized = false;
		private new Camera camera;
		private RenderTexture targetTexture, outputTexture;
		public System.Action<RenderTexture> onRenderingFinished;
		public System.Action<RenderTexture> onTextureChanged;
		#endregion

		#region Properties
		public Camera Camera => camera;
		public RenderTexture OutputTexture => outputTexture;
		#endregion

		#region Life cycle
		protected void Start() {
			if(initialized)
				return;
			initialized = true;
			camera = GetComponent<Camera>();
			UpdateTextures();
			RenderPipelineManager.beginCameraRendering += OnSRPCameraPreRender;
			RenderPipelineManager.endCameraRendering += OnSRPCameraPostRender;
		}

		protected void OnDestroy() {
			camera.targetTexture = null;

			targetTexture?.Destroy();
			targetTexture = null;

			outputTexture?.Destroy();
			outputTexture = null;
		}

		protected void OnPreRender() {
			UpdateTextures();
		}

		protected void OnPostRender() {
			onRenderingFinished?.Invoke(targetTexture);
			Graphics.Blit(targetTexture, outputTexture);
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
		private void UpdateTextures() {
			bool needsRedirecting = false;
			if(targetTexture == null) {
				targetTexture = RenderUtility.CreateScreenSizedRT(RenderTextureFormat.Default);
				needsRedirecting = true;
			}
			else if(RenderUtility.Resize(ref targetTexture, RenderUtility.ScreenSize))
				needsRedirecting = true;
			if(needsRedirecting) {
				outputTexture.Destroy();
				outputTexture = targetTexture.Duplicate();
				camera.targetTexture = targetTexture;
				onTextureChanged?.Invoke(outputTexture);
			}
		}
		#endregion
	}
}
