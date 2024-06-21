using UnityEngine;

namespace NaniCore.Bordure {
	public class SpawnPoint : MonoBehaviour {
		public string Name => name;

		[ContextMenu("Move Protagonist Here")]
		public void MoveProtagonistHere() {
			GameManager.Instance.MoveProtagonistToSpawnPoint(this);
		}
	}
}
