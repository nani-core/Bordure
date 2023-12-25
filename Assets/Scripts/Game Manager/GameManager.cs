using UnityEngine;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Life cycle
		protected void Awake() {
			if(!EnsureSingleton())
				return;
			StartDebugUi();
		}

		protected void Update() {
			UpdateDebugUi();
		}

		protected void OnDestroy() {
			EndDebugUi();
			RenderUtility.ReleasePooledResources();
		}
		#endregion
	}
}