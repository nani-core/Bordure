#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Life cycle
		protected void OnEditUpdate() {
			if(Application.isPlaying || this == null)
				return;

			protagonist = InitializeProtagonist();
		}

		protected void OnValidate() {
			EditorApplication.delayCall += OnEditUpdate;
		}
		#endregion
	}
}
#endif