using UnityEngine;

namespace NaniCore.UnityPlayground {
	public partial class Protagonist : MonoBehaviour {
		#region Life cycle
#if UNITY_EDITOR
		protected void OnValidate() {
			if(!Application.isPlaying) {
				ValidateControl();
				ValidateInteraction();
			}
		}
#endif

		protected void Start() {
			StartInteraction();
		}

		protected void Update() {
			UpdateInteraction();
		}
		#endregion
	}
}