using UnityEngine;

namespace NaniCore.Bordure {
	public enum GameState {
		StartMenu, MainGame,
	}

	public partial class GameManager {
		#region Fields
		[System.NonSerialized] public GameState state = GameState.MainGame;
		#endregion

		#region Interfaces
		public void StartGame() {
			SettingsUiIsOpen = false;

			switch(state) {
				case GameState.MainGame:
					break;
				default:
					StartMenuUiIsOpen = false;
					UsesProtagonist = true;
					state = GameState.MainGame;
					break;
			}
		}

		public void OpenStartMenu() {
			SettingsUiIsOpen = false;

			switch(state) {
				case GameState.StartMenu:
					break;
				default:
					UsesProtagonist = false;
					state = GameState.StartMenu;
					break;
			}
			StartMenuUiIsOpen = true;
		}

		public void OpenSettings() {
			switch(state) {
				case GameState.StartMenu:
					StartMenuUiIsOpen = false;
					break;
				case GameState.MainGame:
					Protagonist.enabled = false;
					TimeScale = 0.0f;
					break;
			}
			SettingsUiIsOpen = true;
		}

		public void CloseSettings() {
			SettingsUiIsOpen = false;
			switch(state) {
				case GameState.StartMenu:
					StartMenuUiIsOpen = true;
					break;
				case GameState.MainGame:
					TimeScale = 1.0f;
					Protagonist.enabled = true;
					break;
			}
		}
		#endregion
	}
}