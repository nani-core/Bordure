using UnityEngine;

namespace NaniCore.UnityPlayground {
	public partial class Protagonist : MonoBehaviour {
		#region Singleton
		public static Protagonist instance;
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		protected void OnValidate() {
			if(!Application.isPlaying) {
				ValidateControl();
				ValidateInteraction();
			}
		}
#endif

		protected void OnEnable() {
			instance = this;
		}

		protected void OnDisable() {
			instance = null;
		}

		protected void Start() {
			StartInteraction();
		}

		protected void Update() {
			UpdateInteraction();
		}
		#endregion
	}
}