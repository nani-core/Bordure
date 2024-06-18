using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Fields
		private bool isBeingDestroyed = false;
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
						UsesProtagonist = false;
					}
					TimeScale = 0.0f;
				}
				else {
					TimeScale = 1.0f;
					if(Protagonist != null) {
						UsesProtagonist = wasUsingProtagonist;
					}
				}
			}
		}

		public void StartGame() {
			if(SceneManager.GetSceneByBuildIndex(Settings.gameStartScene).isLoaded) {
				SceneManager.UnloadSceneAsync(Settings.gameStartScene);
			}
		}

		public void QuitGame() {
			// This invokes `Finalize`.
			Destroy(gameObject);

			Application.Quit();
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#endif
		}

		public void RestartGame() {
			UsesProtagonist = false;
			ResetAchievementProgress();
			InvokeOnGameStart.ResetStaticFlag();
			SceneManager.LoadScene(Settings.gameStartScene, LoadSceneMode.Single);
			PauseMenu.StartMenu.ResetToInitialState();
			PauseMenu.OpenStartMenu();
		}
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
			PauseMenu.OnLoaded += () => PauseMenu.OpenStartMenu();
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