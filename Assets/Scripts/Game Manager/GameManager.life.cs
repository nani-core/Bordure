using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaniCore.Bordure {
	public partial class GameManager {
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
			InitializeUi();
		}

#pragma warning disable CS0465
		protected void Finalize() {
			isBeingDestroyed = true;
			FinalizeUi();
			RenderUtility.ReleasePooledResources();
			ReleaseAllTemporaryResources();
		}
#pragma warning restore
		#endregion
	}
}