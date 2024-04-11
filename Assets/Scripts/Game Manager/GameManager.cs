using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField][Expandable] private GameSettings settings;
		[SerializeField] private Level startLevel;
		#endregion

		#region Fields
		private bool isBeingDestroyed = false;
		#endregion

		#region Interfaces
		public GameSettings Settings => settings;
		public bool IsBeingDestroyed => isBeingDestroyed;
		#endregion

		#region Life cycle
		protected void Awake() {
#if UNITY_EDITOR
			if(!Application.isPlaying)
				return;
#endif
			if(!EnsureSingleton())
				return;

			Initialize();
		}

		protected void Initialize() {
			InitializeConstants();
			InitializeLevel();
			InitializeRigidbody();
			InitializeProtagonist();
			InitializeDebugUi();
		}

		protected void Update() {
			UpdateLoopShape();
			UpdateDebugUi();
		}

		protected void OnDestroy() {
			isBeingDestroyed = true;
			FinalizeProtagonist();
			FinalizeDebugUi();
			RenderUtility.ReleasePooledResources();
			ReleaseAllTemporaryResources();
		}
		#endregion
	}
}