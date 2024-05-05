using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public class UiManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Ui startMenu;
		[SerializeField] private Ui settings;
		#endregion

		#region Fields
		private readonly List<Ui> uiEntries = new();
		#endregion

		#region Life cycle
		protected void Start() {
			if(startMenu != null) {
				startMenu.gameObject.SetActive(false);
			}

			if(settings != null) {
				settings.gameObject.SetActive(false);
			}
		}
		#endregion

		#region Input message handler
		protected void OnPause() {
			var game = GameManager.Instance;
			if(!game.GameStarted) {
				if(CurrentUi == startMenu)
					return;
			}
			game.Ui.CloseLastUi();
		}
		#endregion

		#region Interfaces
		public Ui StartMenu => startMenu;
		public Ui Settings => settings;

		public void OpenStartMenu() {
			OpenUi(startMenu);
		}

		public void OpenSettings() {
			OpenUi(settings);
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
			else
				gameObject.SetActive(false);
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
			else
				gameObject.SetActive(true);

			uiEntries.Add(target);
			target.OnEnter();
			target.OnShow();
		}

		private Ui CurrentUi {
			get {
				if(uiEntries.Count <= 0)
					return null;
				return uiEntries[^1];
			}
		}
		#endregion
	}
}