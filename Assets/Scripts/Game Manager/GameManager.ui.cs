using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[Header("Screen")]
		[SerializeField] private UnityEngine.UI.RawImage renderOutput;
		[SerializeField] private UnityEngine.UI.RawImage debugOverlay;

		[Header("UI")]
		[SerializeField] private Ui startMenuUi;
		[SerializeField] private Ui settingsUi;
		#endregion

		#region Fields
		private RenderTexture debugOverlayTexture;
		private List<Ui> uiEntries = new();
		private bool wasUsingProtagonist;
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

		public void OpenStartMenu() {
			OpenUi(startMenuUi);
		}

		public void OpenSettings() {
			OpenUi(settingsUi);
		}

		public void CloseLastUi() {
			if(uiEntries.Count <= 0)
				return;

			var lastUi = uiEntries[^1];
			uiEntries.RemoveAt(uiEntries.Count - 1);

			lastUi.OnHide();
			lastUi.OnExit();

			if(uiEntries.Count > 0)
				uiEntries[^1].OnShow();
		}
		#endregion

		#region Functions
		private void OpenUi(Ui target) {
			if(target == null) {
				Debug.LogWarning("Warning: Cannot open empty UI.");
				return;
			}

			if(uiEntries.Count > 0)
				uiEntries[^1].OnHide();

			uiEntries.Add(target);
			target.OnEnter();
			target.OnShow();
		}
		#endregion
	}
}