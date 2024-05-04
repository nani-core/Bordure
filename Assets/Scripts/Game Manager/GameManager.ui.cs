using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[Header("Screen")]
		[SerializeField] private UnityEngine.UI.RawImage renderOutput;
		[SerializeField] private UnityEngine.UI.RawImage debugOverlay;

		[Header("UI")]
		[SerializeField] private RectTransform startMenuUi;
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

			if(startMenuUi != null) {
				startMenuUi.gameObject.SetActive(false);
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
		public UnityEngine.UI.RawImage RenderOutput => renderOutput;

		public void DrawDebugOverlayFrame(Texture texture, float opacity = 1f) {
			if(debugOverlayTexture == null)
				return;
			debugOverlayTexture.Overlay(texture, opacity);
		}

		public bool StartMenuUiIsOpen {
			get => startMenuUi.gameObject.activeInHierarchy;
			set => startMenuUi.gameObject.SetActive(value);
		}

		public bool SettingsUiIsOpen {
			get => settingsUi.gameObject.activeInHierarchy;
			set => settingsUi.gameObject.SetActive(value);
		}
		#endregion
	}
}