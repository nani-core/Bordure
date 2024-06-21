using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		private static GameManager instance;
		public static GameManager Instance => instance;

		private bool EnsureSingleton() {
			if(Instance != null && Instance != this) {
				Destroy(gameObject);
				return false;
			}

			instance = this;
			// `Object.DontDestroyOnLoad` needs to be called after the scene is loaded.
			StartCoroutine(DontDestroySelfOnLoadCoroutine());
			return true;
		}

		private System.Collections.IEnumerator DontDestroySelfOnLoadCoroutine() {
			yield return new WaitUntil(() => gameObject.scene.isLoaded);
			DontDestroyOnLoad(gameObject);
		}
	}
}