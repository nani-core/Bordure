using UnityEngine;
namespace NaniCore.Bordure {
	public class StartMenu : Menu {
		#region Fields
		[SerializeField] private GameObject startGameButton;
		[SerializeField] private GameObject continueButton;
		[SerializeField] private GameObject startLogo;
		[SerializeField] private GameObject continueLogo;
		[SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;
		#endregion

		#region Interfaces
		public void ResetToInitialState() {
			startGameButton.SetActive(true);
			continueButton.SetActive(false);
			startLogo.SetActive(true);
			continueLogo.SetActive(false);
			playerInput.enabled = false;
		}

		public void OnStart() {
			startGameButton.SetActive(false);
			continueButton.SetActive(true);
			startLogo.SetActive(false);
			continueLogo.SetActive(true);
			playerInput.enabled = true;
		}
		#endregion

		#region Overriden functions
		public override void OnEnter() {
			GameManager.Instance.Paused = true;
		}

		public override void OnExit() {
			GameManager.Instance.Paused = false;
		}
		#endregion
	}
}