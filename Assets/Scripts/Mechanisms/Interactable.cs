using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(Collider))]
	public class Interactable : MonoBehaviour {
		#region Serialized fields
		[SerializeField] protected new Rigidbody rigidbody;
		[SerializeField] protected UnityEvent onFocusEnter;
		[SerializeField] protected UnityEvent onFocusLeave;
		[SerializeField] protected UnityEvent onInteract;
		#endregion

#if UNITY_EDITOR
		protected void OnValidate() {
			rigidbody = GetComponent<Rigidbody>();
		}
#endif

		protected virtual void OnFocusEnter() {
			onFocusEnter?.Invoke();
		}

		protected virtual void OnFocusLeave() {
			onFocusLeave?.Invoke();
		}

		protected virtual void OnInteract() {
			onInteract?.Invoke();
		}
	}
}