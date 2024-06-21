using UnityEngine;
using System.Collections;

namespace NaniCore {
	public class DelayedLogic : Logic {
		#region Serialized fields
		[Min(0)] public float delayTime;
		#endregion

		private IEnumerator InvokingCoroutine() {
			yield return new WaitForSecondsRealtime(delayTime);
			callback.Invoke();
		}

		public override void Invoke() {
			StartCoroutine(InvokingCoroutine());
		}
	}
}