using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public class PauseMenuManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private StartMenu startMenu;
		[SerializeField] private SettingsMenu settings;
		[SerializeField] private RestartMenu restart;
		#endregion

		#region Fields
		private readonly List<Menu> uiStack = new();
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
			GameManager.Instance.PauseMenu.CloseLastUi();
		}
		#endregion

		#region Interfaces
		public System.Action OnLoaded;

		public StartMenu StartMenu => startMenu;
		public SettingsMenu Settings => settings;

		public void OpenStartMenu() {
			OpenUi(startMenu);
		}

		public void OpenSettings() {
			OpenUi(settings);
		}

		public void OpenRestart() {
			OpenUi(restart);
		}

		public void CloseLastUi() {
			if(uiStack.Count <= 0)
				return;

			var lastUi = uiStack[^1];
			uiStack.RemoveAt(uiStack.Count - 1);

			lastUi.OnHide();
			lastUi.OnExit();

			if(uiStack.Count > 0)
				uiStack[^1].OnShow();
			else
				gameObject.SetActive(false);
		}

		public void OpenUi(Menu target) {
			if(target == null) {
				Debug.LogWarning("Warning: Cannot open empty UI.");
				return;
			}

			if(uiStack.Count > 0)
				uiStack[^1].OnHide();
			else
				gameObject.SetActive(true);

			uiStack.Add(target);
			target.OnEnter();
			target.OnShow();
		}

		public Menu CurrentUi {
			get {
				if(uiStack.Count <= 0)
					return null;
				return uiStack[^1];
			}
		}
		#endregion
	}
}