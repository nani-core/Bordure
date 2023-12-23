using UnityEngine;

namespace NaniCore.Loopool {
	public class GameManager : MonoBehaviour {
		#region Singleton
		private static GameManager instance;
		public static GameManager Instance => instance;
		#endregion

		#region Serialized fields
		public UnityEngine.UI.RawImage debugLayer;
		#endregion

		#region Fields
		private Protagonist protagonist;
		private RenderTexture debugFrame;
		#endregion

		#region Properties
		public Protagonist Protagonist => protagonist;
		public Camera MainCamera => Protagonist?.Camera;
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

		public void DrawDebugFrame(Texture texture, float opacity = 1f) {
			if(debugFrame == null)
				return;
			debugFrame.Overlay(texture, opacity);
		}
		#endregion

		#region Message handlers
		protected void OnProtagonistCreated(Protagonist protagonist) {
			if(this.protagonist != null && this.protagonist != protagonist) {
				Destroy(protagonist.gameObject);
				return;
			}
			this.protagonist = protagonist;
		}

		protected void OnProtagonistDestroyed(Protagonist protagonist) {
			if(protagonist != this.protagonist)
				return;
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
			if(debugLayer != null) {
				debugLayer.enabled = true;
				debugLayer.texture = debugFrame = RenderUtility.CreateScreenSizedRT();
				debugFrame.SetValue(Color.clear);
			}
		}

		protected void Update() {
			debugFrame?.SetValue(Color.clear);
		}

		protected void OnDisable() {
			debugFrame?.Destroy();
			RenderUtility.ReleasePooledResources();
		}
		#endregion
	}
}