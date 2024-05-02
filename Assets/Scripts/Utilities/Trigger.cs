using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	public class Trigger : MonoBehaviour {
		#region Serialized fields
		public UnityEvent onEnter;
		public UnityEvent onExit;
		#endregion

		#region Life cycle
		protected void OnTriggerEnter(Collider other) {
			Debug.Log($"{other} entered {this}.", this);
			onEnter?.Invoke();
		}

		protected void OnTriggerExit(Collider other) {
			Debug.Log($"{other} exit {this}.", this);
			onExit?.Invoke();
		}
		#endregion
	}
}
