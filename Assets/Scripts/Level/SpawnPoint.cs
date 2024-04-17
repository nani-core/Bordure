using UnityEngine;

namespace NaniCore.Bordure {
	public class SpawnPoint : MonoBehaviour {
		public string Name => name;

		public Level Level => GetComponentInParent<Level>(true);
	}
}
