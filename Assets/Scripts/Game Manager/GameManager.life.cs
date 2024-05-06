using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[SerializeField] private UnityEvent onStart;
		#endregion

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
			onStart?.Invoke();
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
		protected void Initialize() {
			InitializeConstants();
			InitializeLevel();
			InitializePhysics();
			InitializeDebug();
			Ui.OnLoaded += () => Ui.OpenStartMenu();
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