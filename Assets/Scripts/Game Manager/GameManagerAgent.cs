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

		public void MoveProtagonistToSpawnPoint(SpawnPoint spawnPoint) {
			Instance.MoveProtagonistToSpawnPoint(spawnPoint);
		}

		public void MoveProtagonistToSpawnPointByName(string name) {
			Instance.MoveProtagonistToSpawnPointByName(name);
		}
		#endregion

		#region Camera
		public void AlignCameraTo(Transform transform) {
			Instance.AlignCameraTo(transform);
		}
		#endregion

		#region Level
		public void LoadLevel(Level template) {
			Instance.LoadLevel(template);
		}

		public void UnloadLevelByName(string levelName) {
			Instance.UnloadLevelByName(levelName);
		}
		#endregion

		#region Game
		public void QuitGame() {
			Instance.QuitGame();
		}
		#endregion
	}
}
