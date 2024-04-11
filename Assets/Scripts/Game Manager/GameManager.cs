using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Bordure {
	[ExecuteInEditMode]
	public partial class GameManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField][Expandable] private GameSettings settings;
		[SerializeField] private Level startLevel;
		#endregion

		#region Interfaces
		public GameSettings Settings => settings;
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
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				OnEditUpdate();
				return;
			}
#endif
			UpdateLoopShape();
			UpdateDebugUi();
		}

		protected void OnDestroy() {
#if UNITY_EDITOR
			if(!Application.isPlaying)
				return;
#endif
			FinalizeProtagonist();
			FinalizeDebugUi();
			RenderUtility.ReleasePooledResources();
			ReleaseAllTemporaryResources();
		}
		#endregion
	}
}