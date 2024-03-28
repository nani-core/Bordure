using UnityEngine;

namespace NaniCore.Bordure {
	public class Level : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private SpawnPoint spawnPoint;
#if DEBUG
		[SerializeField] private SpawnPoint debugSpawnPoint;
#endif
		#endregion

		#region Interfaces
		public delegate void LevelCallback(Level self);
		public LevelCallback onLoaded, onUnloaded;

		public SpawnPoint SpawnPoint => spawnPoint;

		public SpawnPoint DebugSpawnPoint {
			get {
#if DEBUG
				return debugSpawnPoint ?? spawnPoint;
#else
				return spawnPoint;
#endif
			}
		}
		#endregion

		#region Life cycle
		protected void Start() {
			onLoaded?.Invoke(this);
		}

		protected void OnDestroy() {
			onUnloaded?.Invoke(this);
		}
		#endregion
	}
}
