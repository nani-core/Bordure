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
		private bool isFirstCycle = true;
		private float runStartTime;
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

		// Called when clicking the start button in the pause menu.
		public void StartGame() {
			ResetStates();
		}

		public void QuitGame() {
			// This invokes `Finalize`.
			Destroy(gameObject);

			Application.Quit();
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#endif
		}

		// Called after the previous game ends and before reloading the start scene.
		public void RestartGame() {
			// Disable play controls.
			UsesProtagonist = false;

			ResetStates();

			// Reload scenes.
			SceneManager.LoadScene(Settings.gameStartScene, LoadSceneMode.Single);
			PauseMenu.StartMenu.ResetToInitialState();
			PauseMenu.OpenStartMenu();
		}

		public void FinishGame() {
			UsesProtagonist = false;
			PauseMenu.OpenRestart();
			if(isFirstCycle) {
				isFirstCycle = false;
				float runTime = Time.time - runStartTime;
				FinishSpeedrunAchievement(runTime);
			}
		}

		public void UnloadStartScene() {
			if(SceneManager.GetSceneByBuildIndex(Settings.gameStartScene).isLoaded)
				SceneManager.UnloadSceneAsync(Settings.gameStartScene);
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

		private void ResetStates() {
			ResetAchievementProgress();
			isFirstCycle = true;
			runStartTime = Time.time;
			InvokeOnGameStart.ResetStaticFlag();
		}
		#endregion
	}
}