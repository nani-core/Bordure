using UnityEngine;

namespace NaniCore.Bordure {
	public class TimedHider : MonoBehaviour {
		[Min(0)] public float time = 5f;

		protected void OnShown() {
			enabled = true;
			StartCoroutine(TimerCoroutine());
		}

		protected void OnHidden() {
			StopAllCoroutines();
		}

		private System.Collections.IEnumerator TimerCoroutine() {
			yield return new WaitForSeconds(time);
			SendMessage("Hide");
		}
	}
}