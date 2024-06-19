using UnityEngine;

namespace NaniCore.Bordure {
	public class InvokeOnGameStart : MonoBehaviour {
		#region Life cycle
		protected void Start() {
			StartCoroutine(OnStartCoroutine());
		}
		#endregion

		#region Internal
		private static bool isOnGameStart = true;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void ResetStaticFlag() {
			isOnGameStart = true;
		}

		private System.Collections.IEnumerator OnStartCoroutine() {
			yield return new WaitForEndOfFrame();
			if(TryGetComponent(out Logic logic)) {
				if(isOnGameStart) {
					logic.Invoke();
					isOnGameStart = false;
				}
			}
		}
		#endregion
	}
}
