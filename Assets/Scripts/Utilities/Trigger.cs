using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	public class Trigger : MonoBehaviour {
		#region Serialized fields
		public bool oneTime;
		public UnityEvent onEnter;
		public UnityEvent onExit;
		#endregion

		#region Life cycle
		protected void OnTriggerEnter(Collider other) {
			onEnter?.Invoke();
			if(oneTime) {
				Destroy(this);
			}
		}

		protected void OnTriggerExit(Collider other) {
			onExit?.Invoke();
		}
		#endregion
	}
}
