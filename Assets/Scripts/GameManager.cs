using UnityEngine;

namespace NaniCore.Loopool {
	public class GameManager : MonoBehaviour {
		#region Singleton
		private static GameManager instance;
		public static GameManager Instance => instance;
		#endregion

		#region Fields
		private RenderTexture worldView;
		#endregion

		#region Properties
		public RenderTexture WorldView => worldView;
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

		protected void Start() {
			worldView = RenderUtility.CreateScreenSizedRT();
		}

		protected void Update() {
			if(Camera.main != null) {
				worldView.Capture(Camera.main);
			}
		}

		protected void OnApplicationQuit() {
			worldView.Destroy();
			RenderUtility.ReleasePooledResources();
		}
		#endregion
	}
}