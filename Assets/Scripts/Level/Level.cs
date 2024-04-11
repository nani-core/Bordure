using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	public class Level : MonoBehaviour {
		#region Serialized fields
		public bool spawnProtagonistAtStart = true;
		[SerializeField] private SpawnPoint spawnPoint;
#if DEBUG
		[SerializeField] private SpawnPoint debugSpawnPoint;
#endif

		[SerializeField] private UnityEvent onLoaded;
		#endregion

		#region Interfaces
		public System.Action OnLoaded, OnUnloaded;

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
			OnLoaded += onLoaded.Invoke;
			OnLoaded?.Invoke();
		}

		protected void OnDestroy() {
			OnUnloaded?.Invoke();
		}
		#endregion
	}
}
