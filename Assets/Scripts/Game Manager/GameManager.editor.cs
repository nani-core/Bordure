#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Life cycle
		protected void OnEditUpdate() {
			if(Application.isPlaying || this == null)
				return;

			InitializeProtagonistInEditMode();
		}

		protected void OnValidate() {
			EditorApplication.delayCall += OnEditUpdate;
		}
		#endregion
	}
}
#endif