using UnityEngine;

namespace NaniCore.Bordure {
	using static GameManager;

	[CreateAssetMenu(menuName = "Nani Core/Game Manager Agent")]
	public class GameManagerAgent : ScriptableObject {
		#region Protagonist
		public void StartUsingProtagonist() {
			Instance.UsesProtagonist = true;
		}

		public void StopUsingProtagonist() {
			Instance.UsesProtagonist = false;
		}

		public void MoveProtagonistToSpawnPoint(SpawnPoint spawnPoint) {
			Instance.MoveProtagonistToSpawnPoint(spawnPoint);
		}

		public void MoveProtagonistToSpawnPointByName(string name) {
			Instance.MoveProtagonistToSpawnPointByName(name);
		}

		public bool UsesProtagonistMovement {
			get => Instance.UsesProtagonistMovement;
			set => Instance.UsesProtagonistMovement = value;
		}

		public bool UsesProtagonistOrientation {
			get => Instance.UsesProtagonistOrientation;
			set => Instance.UsesProtagonistOrientation = value;
		}

		public bool ProtagonistIsKinematic {
			get => Instance.ProtagonistIsKinematic;
			set => Instance.ProtagonistIsKinematic = value;
		}

		public void ProtagonistSitOn(Seat seat) {
			Instance.ProtagonistSitOn(seat);
		}

		public void ProtagonistLeaveSeat() {
			Instance.ProtagonistLeaveSeat();
		}
		#endregion

		#region Camera
		public void AlignCameraTo(Transform target) {
			Instance.AlignCameraTo(target);
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
