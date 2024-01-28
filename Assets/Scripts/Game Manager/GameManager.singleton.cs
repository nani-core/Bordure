using UnityEngine;

namespace NaniCore.Stencil {
	public partial class GameManager : MonoBehaviour {
		private static GameManager instance;
		public static GameManager Instance => instance;

		private bool EnsureSingleton() {
			if(Instance != null && Instance != this) {
				Destroy(gameObject);
				return false;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);
			return true;
		}
	}
}