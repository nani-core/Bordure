using UnityEngine;

namespace NaniCore.Loopool {
	public class GameManager : MonoBehaviour {
		#region Singleton
		private static GameManager instance;
		public static GameManager Instance => instance;
		#endregion

		#region Life cycle
		protected void Awake() {
			if(Instance != null) {
				// Not Destroy(gameObject) as other managers might be in the
				// children hierarchy, we don't want them to be destroyed.
				Destroy(this);
				return;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		protected void OnApplicationQuit() {
			RenderUtility.ReleasePooledResources();
		}
		#endregion
	}
}