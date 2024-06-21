using UnityEngine;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Camera))]
	public class MainCameraManager : MonoBehaviour {
		#region Fields
		private new Camera camera;
		private CameraRenderingTarget renderingTarget;
		public System.Action<RenderTexture> onRenderingFinished;
		#endregion

		#region Properties
		public Camera Camera {
			get {
				if(camera == null)
					camera = GetComponent<Camera>();
				return camera;
			}
		}
		#endregion

		#region Life cycle
		protected void Start() {
			renderingTarget = transform.EnsureComponent<CameraRenderingTarget>();
			renderingTarget.onTextureChanged = texture => GameManager.Instance.RenderOutput.texture = texture;
			renderingTarget.onRenderingFinished += OnRenderingFinished;
		}
		#endregion

		#region Event handlers
		private void OnRenderingFinished(RenderTexture result) {
			onRenderingFinished?.Invoke(result);
		}
		#endregion
	}
}
