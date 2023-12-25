#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Life cycle
		protected void OnEditorAwake() {
			if(Application.isPlaying)
				return;
		}

		protected void OnEditorUpdate() {
			if(Application.isPlaying)
				return;

			InitializeProtagonist();
		}

		protected void OnEditorDestroy() {
			if(Application.isPlaying)
				return;
		}

		protected void OnValidate() {
			if(Application.isPlaying)
				return;

			EditorApplication.delayCall += OnEditorUpdate;
		}
		#endregion
	}
}
#endif