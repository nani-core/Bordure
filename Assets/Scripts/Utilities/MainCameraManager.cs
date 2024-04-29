using UnityEngine;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Camera))]
	public class MainCameraManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private UnityEngine.UI.RawImage renderOutputImage;
		#endregion

		#region Fields
		private new Camera camera;
		private CameraRenderingTarget renderingTarget;
		public System.Action<RenderTexture> onRenderingFinished;
		#endregion

		#region Properties
		public Camera Camera => camera;
		#endregion

		#region Life cycle
		protected void Start() {
			camera = GetComponent<Camera>();

			renderingTarget = transform.EnsureComponent<CameraRenderingTarget>();
			renderingTarget.onTextureChanged = texture => renderOutputImage.texture = texture;
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
