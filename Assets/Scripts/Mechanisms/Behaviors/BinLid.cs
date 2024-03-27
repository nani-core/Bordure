using UnityEngine;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(FocusValidator))]
	public class BinLid : MonoBehaviour {
		#region Fields
		private FocusValidator validator;
		private Coroutine rollingCoroutine;
		#endregion

		#region Interfaces
		private FocusValidator Validator {
			get {
				if(validator == null)
					validator = GetComponent<FocusValidator>();
				return validator;
			}
		}

		public void Roll() {
			if(rollingCoroutine != null) {
				Debug.LogWarning($"{this} is already rolling. Cancelling new rolling request.");
				return;
			}
			rollingCoroutine = StartCoroutine(RollingCoroutine());
		}
		#endregion

		#region Functions
		private System.Collections.IEnumerator RollingCoroutine() {
			// TODO: dummy
			Validator.enabled = false;
			Quaternion startOrientation = transform.rotation;
			float duration = 1f;
			for(float startTime = Time.time, t; (t = (Time.time - startTime) / duration) < 1;) {
				SetRollingProgress(startOrientation, t);
				yield return new WaitForFixedUpdate();
			}
			SetRollingProgress(startOrientation, 1f);
			Validator.enabled = true;
			rollingCoroutine = null;
		}

		private void SetRollingProgress(Quaternion startOrientation, float t) {
			Quaternion rotation = default;
			if(t != 1) {
				rotation = Quaternion.Euler(Vector3.right * (360f * t));
			}
			transform.rotation = startOrientation * rotation;
		}
		#endregion
	}
}
