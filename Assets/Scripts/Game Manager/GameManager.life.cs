using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Fields
		private bool isBeingDestroyed = false;
		private bool gameStarted = false;
		private bool wasUsingProtagonist;
		#endregion

		#region Interfaces
		public bool IsBeingDestroyed => isBeingDestroyed;

		public bool Paused {
			get => Time.timeScale > 0.0f;
			set {
				if(value) {
					if(Protagonist != null) {
						wasUsingProtagonist = UsesProtagonist;
						Protagonist.enabled = false;
					}
					TimeScale = 0.0f;
				}
				else {
					TimeScale = 1.0f;
					if(Protagonist != null) {
						Protagonist.enabled = wasUsingProtagonist;
					}
				}
			}
		}

		public void StartGame() {
			gameStarted = true;
			Ui.CloseLastUi();
			UsesProtagonist = true;
		}

		public void QuitGame() {
			// This invokes `Finalize`.
			Destroy(gameObject);

			Application.Quit();
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#endif
		}

		public bool GameStarted => gameStarted;
		#endregion

		#region Functions
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void InstantiateOnGameStart() {
			if(FindObjectOfType<DontSpawnGameManager>() != null)
				return;

			GameObject prefab = Resources.Load<GameObject>("Game Manager");
			if(prefab == null || !prefab.TryGetComponent<GameManager>(out _)) {
				throw new UnityException("Error: Failed to instantiate the game manager.");
			}

			var instance = Instantiate(prefab);
			instance.name = prefab.name;
		}

		protected void Initialize() {
			InitializeConstants();
			InitializeLevel();
			InitializePhysics();
			InitializeDebug();
			Ui.OnLoaded += () => Ui.OpenStartMenu();
			eventSystem.gameObject.SetActive(true);
		}

#pragma warning disable CS0465
		protected void Finalize() {
			isBeingDestroyed = true;
			FinalizeDebug();
			RenderUtility.ReleasePooledResources();
			ReleaseAllTemporaryResources();
		}
#pragma warning restore
		#endregion
	}
}