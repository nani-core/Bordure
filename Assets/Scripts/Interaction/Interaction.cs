using UnityEngine;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(Collider))]
	public abstract class Interaction : MonoBehaviour {
		[SerializeField] protected new Rigidbody rigidbody;

		protected void OnValidate() {
			rigidbody = GetComponent<Rigidbody>();
		}

		protected abstract void OnFocusEnter();
		protected abstract void OnFocusLeave();
		protected abstract void OnInteract();
	}
}