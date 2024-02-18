#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NaniCore.Stencil {
	public partial class Elevator : MonoBehaviour {
		#region Life cycle
		protected void OnValidate() {
			if(Application.isPlaying)
				return;

			EditorApplication.delayCall += () => {
				if(Application.isPlaying)
					return;
				UpdateButtons();
				foreach(var button in buttonAnchor.Children()) {
					button.gameObject.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.DontUnloadUnusedAsset;
				}
			};
		}
		#endregion
	}
}
#endif