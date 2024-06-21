using UnityEngine;
using UnityEngine.Events;

namespace NaniCore {
	public class ColliderEvents : MonoBehaviour {
		#region Serialized fields
		public LayerMask layerMask;
		public UnityEvent onEnter;
		public UnityEvent onExit;
		#endregion

		#region Life cycle
		protected void OnCollisionEnter(Collision collision) {
			if(((1 << collision.gameObject.layer) & layerMask) == 0)
				return;

			onEnter?.Invoke();
		}

		protected void OnCollisionExit(Collision collision) {
			if(((1 << collision.gameObject.layer) & layerMask) == 0)
				return;

			onExit?.Invoke();
		}
		#endregion
	}
}
