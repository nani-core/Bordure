using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public class PauseMenuManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Menu startMenu;
		[SerializeField] private Menu settings;
		#endregion

		#region Fields
		private readonly List<Menu> uiEntries = new();
		#endregion

		#region Life cycle
		protected void Start() {
			// Disable all UI.
			foreach(var child in transform.Children()) {
				if(!child.TryGetComponent<Menu>(out var ui))
					continue;
				ui.gameObject.SetActive(false);
			}

			OnLoaded?.Invoke();
		}
		#endregion

		#region Input message handler
		protected void OnPause() {
			var game = GameManager.Instance;
			if(!game.GameStarted) {
				if(CurrentUi == startMenu)
					return;
			}
			game.PauseMenu.CloseLastUi();
		}
		#endregion

		#region Interfaces
		public System.Action OnLoaded;

		public Menu StartMenu => startMenu;
		public Menu Settings => settings;

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

		public void OpenUi(Menu target) {
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

		public Menu CurrentUi {
			get {
				if(uiEntries.Count <= 0)
					return null;
				return uiEntries[^1];
			}
		}
		#endregion
	}
}