using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public struct UiEntry {
		public System.Action onEnter, onExit;
		public System.Action onShow, onHide;
	}

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
		private List<KeyValuePair<Object, UiEntry>> uiEntries = new();
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
			OpenUi(startMenuUi, new UiEntry() {
				onEnter = () => {
					Paused = true;
				},
				onExit = () => {
					Paused = false;
				},
				onShow = () => {
					startMenuUi.gameObject.SetActive(true);
				},
				onHide = () => {
					startMenuUi.gameObject.SetActive(false);
				},
			});
		}

		public void OpenSettings() {
			OpenUi(settingsUi, new UiEntry() {
				onShow = () => {
					settingsUi.gameObject.SetActive(true);
				},
				onHide = () => {
					settingsUi.gameObject.SetActive(false);
				},
			});
		}

		public void CloseLastUi() {
			if(uiEntries.Count <= 0)
				return;

			var lastUi = uiEntries[^1];
			uiEntries.RemoveAt(uiEntries.Count - 1);

			lastUi.Value.onHide?.Invoke();
			lastUi.Value.onExit?.Invoke();

			if(uiEntries.Count > 0)
				uiEntries[^1].Value.onShow?.Invoke();
		}
		#endregion

		#region Functions
		private bool Paused {
			get => Time.timeScale > 0.0f;
			set {
				if(value) {
					if(Protagonist != null) {
						wasUsingProtagonist = UsesProtagonist;
						Protagonist.enabled = false;
					}
					TimeScale = 0.0f;
				}
				else {
					TimeScale = 1.0f;
					if(Protagonist != null) {
						Protagonist.enabled = wasUsingProtagonist;
					}
				}
			}
		}

		private void OpenUi(Object target, UiEntry entry) {
			if(target == null) {
				Debug.LogWarning("Warning: Cannot open empty UI.");
				return;
			}

			if(uiEntries.Count > 0)
				uiEntries[^1].Value.onHide.Invoke();

			uiEntries.Add(new(target, entry));
			entry.onEnter?.Invoke();
			entry.onShow?.Invoke();
		}
		#endregion
	}
}