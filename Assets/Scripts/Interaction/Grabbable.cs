using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(Collider))]
	public class Grabbable : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private UnityEvent onGrabBegin;
		[SerializeField] private UnityEvent onGrabEnd;
		#endregion

		#region Fields
		private new Rigidbody rigidbody;
		#endregion

		#region Grabbing
		private bool isKinematic = false;
		private RigidbodyConstraints originalConstraints;
		private Transform originalParent;

		public bool IsKinematic {
			get => isKinematic;
			set {
				if(rigidbody == null)
					return;
				if(value == isKinematic)
					return;
				if(value) {
					originalConstraints = rigidbody.constraints;
					originalParent = transform.parent;

					rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					transform.SetParent(null);
				}
				else {
					rigidbody.constraints = originalConstraints;
					transform.SetParent(originalParent);

					originalConstraints = RigidbodyConstraints.None;
					originalParent = null;
				}
				isKinematic = value;
			}
		}

		protected void OnInteract() {
			Protagonist.instance.GrabbingObject = this;
		}

		protected void OnGrabBegin() {
			IsKinematic = true;
			onGrabBegin.Invoke();
		}

		protected void OnGrabEnd() {
			IsKinematic = false;
			onGrabEnd.Invoke();
		}
		#endregion

		#region Life cycle
		protected void Start() {
			rigidbody = GetComponent<Rigidbody>();
		}

		protected void OnCollisionEnter(Collision _) {
			var protagonist = Protagonist.instance;
			if(protagonist.GrabbingObject == this)
				protagonist.GrabbingObject = null;
		}
		#endregion
	}
}