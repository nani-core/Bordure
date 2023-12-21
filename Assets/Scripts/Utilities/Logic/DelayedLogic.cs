using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace NaniCore {
	public class DelayedLogic : MonoBehaviour {
		[Min(0)] public float delayTime;
		public UnityEvent callback;

		private IEnumerator InvokingCoroutine() {
			yield return new WaitForSeconds(delayTime);
			callback.Invoke();
		}

		public void Invoke() {
			StartCoroutine(InvokingCoroutine());
		}
	}
}