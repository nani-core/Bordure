using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private UnityEngine.UI.RawImage debugOverlayImage;
		#endregion

		#region Fields
		private RenderTexture debugOverlayTexture;
		#endregion

		#region Interfaces
		public void DrawDebugOverlayFrame(Texture texture, float opacity = 1f) {
			if(debugOverlayTexture == null)
				return;
			debugOverlayTexture.Overlay(texture, opacity);
		}
		#endregion

		#region Life cycle
		protected void InitializeDebugUi() {
			if(debugOverlayImage == null)
				return;

			debugOverlayImage.enabled = true;
			debugOverlayImage.texture = debugOverlayTexture = RenderUtility.CreateScreenSizedRT();
			debugOverlayTexture.SetValue(Color.clear);
		}

		protected void UpdateDebugUi() {
			debugOverlayTexture?.SetValue(Color.clear);
		}

		protected void FinalizeDebugUi() {
			debugOverlayTexture?.Destroy();
		}
		#endregion
	}
}