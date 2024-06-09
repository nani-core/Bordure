using UnityEngine;

namespace NaniCore.Bordure {
	public class InvokeOnGameStart : MonoBehaviour {
		#region Life cycle
		protected void Start() {
			if(!isOnGameStart)
				return;

			if(!TryGetComponent(out Logic logic))
				return;

			logic.Invoke();
			SetFlagOnNextFrame();
		}
		#endregion

		#region Internal
		private static bool isOnGameStart = true;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void ResetStaticFlag() {
			isOnGameStart = true;
		}

		private System.Collections.IEnumerator SetFlagOnNextFrameCoroutine() {
			yield return new WaitForEndOfFrame();
			isOnGameStart = false;
		}

		private void SetFlagOnNextFrame() {
			StartCoroutine(SetFlagOnNextFrameCoroutine());
		}
		#endregion
	}
}
