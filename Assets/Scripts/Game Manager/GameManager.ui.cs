using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[Header("UI")]
		[SerializeField] private UnityEngine.UI.RawImage renderOutput;
		[SerializeField] private UnityEngine.UI.RawImage debugOverlay;
		[SerializeField] private RectTransform settingsUi;
		#endregion

		#region Fields
		private RenderTexture debugOverlayTexture;
		#endregion

		#region Life cycle
		protected void InitializeUi() {
			if(renderOutput != null) {
				renderOutput.gameObject.SetActive(true);
			}

			if(debugOverlay != null) {
				debugOverlay.gameObject.SetActive(true);
				debugOverlay.texture = debugOverlayTexture = RenderUtility.CreateScreenSizedRT();
				debugOverlayTexture.SetValue(Color.clear);
			}

			if(settingsUi != null) {
				settingsUi.gameObject.SetActive(false);
			}
		}

		protected void UpdateUi() {
			if(debugOverlayTexture != null) {
				debugOverlayTexture.SetValue(Color.clear);
			}
		}

		protected void FinalizeUi() {
			if(debugOverlayTexture != null) {
				debugOverlayTexture.Destroy();
			}
		}
		#endregion

		#region Interfaces
		public void DrawDebugOverlayFrame(Texture texture, float opacity = 1f) {
			if(debugOverlayTexture == null)
				return;
			debugOverlayTexture.Overlay(texture, opacity);
		}

		public bool SettingsUiIsOpen {
			get => settingsUi.gameObject.activeSelf;
			set {
				if(value) {
					Protagonist.enabled = false;
					TimeScale = 0.0f;
					settingsUi.gameObject.SetActive(true);
				}
				else {
					settingsUi.gameObject.SetActive(false);
					TimeScale = 1.0f;
					Protagonist.enabled = true;
				}
			}
		}
		#endregion
	}
}