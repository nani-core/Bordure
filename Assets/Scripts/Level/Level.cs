using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	public class Level : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private SpawnPoint spawnPoint;
#if DEBUG
		[SerializeField] private SpawnPoint debugSpawnPoint;
#endif
		#endregion

		#region Interfaces
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
	}
}
