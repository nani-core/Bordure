using UnityEngine;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(Collider))]
	public abstract class Interaction : MonoBehaviour {
		[SerializeField] protected new Rigidbody rigidbody;

		protected void OnValidate() {
			rigidbody = GetComponent<Rigidbody>();
		}

		public abstract void OnFocusEnter();
		public abstract void OnFocusLeave();
		public abstract void OnInteract();
	}
}