using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Loopool {
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

			StartConstants();
			protagonist = InitializeProtagonist();
			StartDebugUi();
		}

		protected void Update() {
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				OnEditUpdate();
				return;
			}
#endif
			UpdateDebugUi();
		}

		protected void OnDestroy() {
#if UNITY_EDITOR
			if(!Application.isPlaying)
				return;
#endif
			EndDebugUi();
			RenderUtility.ReleasePooledResources();
		}
		#endregion
	}
}