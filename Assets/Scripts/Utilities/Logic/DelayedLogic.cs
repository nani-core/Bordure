using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace NaniCore {
	public class DelayedLogic : Logic {
		[Min(0)] public float delayTime;
		public UnityEvent callback;

		private IEnumerator InvokingCoroutine() {
			yield return new WaitForSeconds(delayTime);
			callback.Invoke();
		}

		public override void Invoke() {
			StartCoroutine(InvokingCoroutine());
		}
	}
}