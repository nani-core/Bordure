using UnityEngine;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(Collider))]
	public abstract class Interactable : MonoBehaviour {
		[SerializeField] protected new Rigidbody rigidbody;

#if UNITY_EDITOR
		protected void OnValidate() {
			rigidbody = GetComponent<Rigidbody>();
		}
#endif

		protected abstract void OnFocusEnter();
		protected abstract void OnFocusLeave();
		protected abstract void OnInteract();
	}
}