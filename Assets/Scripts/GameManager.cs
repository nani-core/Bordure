using UnityEngine;

namespace NaniCore.Loopool {
	public class GameManager : MonoBehaviour {
		#region Singleton
		private static GameManager instance;
		public static GameManager Instance => instance;
		#endregion

		#region Serialized fields
		public Camera mainCamera;
		#endregion

		#region Fields
		private Protagonist protagonist;
		#endregion

		#region Properties
		public Protagonist Protagonist => protagonist;
		#endregion

		#region Functions
		private bool EnsureSingleton() {
			if(Instance != null && Instance != this) {
				Destroy(gameObject);
				return false;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);
			return true;
		}
		#endregion

		#region Message handlers
		protected void OnProtagonistCreated(Protagonist protagonist) {
			if(this.protagonist != null && this.protagonist != protagonist) {
				Destroy(protagonist.gameObject);
				return;
			}
			this.protagonist = protagonist;
			mainCamera.transform.SetParent(protagonist.Eye, false);
			mainCamera.transform.localPosition = Vector3.zero;
			mainCamera.transform.localRotation = Quaternion.identity;
			mainCamera.transform.localScale = Vector3.one;
		}

		protected void OnProtagonistDestroyed(Protagonist protagonist) {
			if(protagonist != this.protagonist)
				return;
			mainCamera.transform.SetParent(transform, true);
			this.protagonist = null;
		}
		#endregion

		#region Life cycle
		protected void Awake() {
			if(!EnsureSingleton())
				return;
		}

		protected void OnEnable() {
			if(!EnsureSingleton())
				return;
		}

		protected void OnDisable() {
			RenderUtility.ReleasePooledResources();
		}
		#endregion
	}
}