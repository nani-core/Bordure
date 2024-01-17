using UnityEngine;

namespace NaniCore.Loopool {
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

		public SpawnPoint SpawnPoint {
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
