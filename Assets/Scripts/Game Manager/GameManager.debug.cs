using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[Header("Debug")]
		[SerializeField] private UnityEngine.UI.RawImage renderOutput;
		[SerializeField] private UnityEngine.UI.RawImage debugOverlay;
		#endregion

		#region Fields
		private RenderTexture debugOverlayTexture;
		#endregion

		#region Life cycle
		protected void InitializeDebug() {
			if(renderOutput != null) {
				renderOutput.gameObject.SetActive(true);
			}

			if(debugOverlay != null) {
				debugOverlay.gameObject.SetActive(true);
				debugOverlay.texture = debugOverlayTexture = RenderUtility.CreateScreenSizedRT();
				debugOverlayTexture.SetValue(Color.clear);
			}
		}

		protected void UpdateDebug() {
			if(debugOverlayTexture != null) {
				debugOverlayTexture.SetValue(Color.clear);
			}
		}

		protected void FinalizeDebug() {
			if(debugOverlayTexture != null) {
				debugOverlayTexture.Destroy();
			}
		}
		#endregion

		#region Interfaces
		public UnityEngine.UI.RawImage RenderOutput => renderOutput;

		public void DrawDebugOverlayFrame(Texture texture, float opacity = 1f) {
			if(debugOverlayTexture == null)
				return;
			debugOverlayTexture.Overlay(texture, opacity);
		}
		#endregion
	}
}