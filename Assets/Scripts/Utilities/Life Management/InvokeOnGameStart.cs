using UnityEngine;

namespace NaniCore.Bordure {
	public class InvokeOnGameStart : MonoBehaviour {
		#region Life cycle
		protected void Start() {
			if(!isOnGameStart)
				return;

			StartCoroutine(OnStartCoroutine());
		}
		#endregion

		#region Internal
		private static bool isOnGameStart = true;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void ResetStaticFlag() {
			isOnGameStart = true;
		}

		private System.Collections.IEnumerator OnStartCoroutine() {
			if(TryGetComponent(out Logic logic)) {
				yield return new WaitForEndOfFrame();
				logic.Invoke();
			}
			yield return new WaitForEndOfFrame();
			isOnGameStart = false;
		}
		#endregion
	}
}
