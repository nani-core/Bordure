using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private bool isBeingDestroyed = false;
		#endregion

		#region Interfaces
		public bool IsBeingDestroyed => isBeingDestroyed;

		public void QuitGame() {
			// This invokes `Finalize`.
			Destroy(gameObject);

			Application.Quit();
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#endif
		}
		#endregion

		#region Functions
		protected void Initialize() {
			InitializeConstants();
			InitializeLevel();
			InitializeRigidbody();
			InitializeProtagonist();
			InitializeDebugUi();
		}

		protected void Finalize() {
			isBeingDestroyed = true;
			FinalizeProtagonist();
			FinalizeDebugUi();
			RenderUtility.ReleasePooledResources();
			ReleaseAllTemporaryResources();
		}
		#endregion
	}
}