using UnityEngine;

namespace NaniCore.Bordure {
	using static GameManager;

	[CreateAssetMenu(menuName = "Nani Core/Game Manager Agent")]
	public class GameManagerAgent : ScriptableObject {
		#region Protagonist
		public void StartUsingProtagonist() {
			Instance.IsUsingProtagonist = true;
		}

		public void StopUsingProtagonist() {
			Instance.IsUsingProtagonist = false;
		}
		#endregion
	}
}
