using UnityEngine;

namespace NaniCore.Bordure {
	public class UiManager : MonoBehaviour {
		#region Input message handler
		protected void OnPause() {
			var game = GameManager.Instance;
			if(game.GameStarted)
				game.CloseLastUi();
		}
		#endregion
	}
}