using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Loopool {
	public class Grabbable : Interactable {
		#region Serialized fields
		[SerializeField] private UnityEvent onGrabBegin;
		[SerializeField] private UnityEvent onGrabEnd;
		#endregion

		#region Inherited interface
		protected override void OnInteract() {
			base.OnInteract();
			Protagonist.instance.GrabbingObject = this;
		}
		#endregion

		#region Grabbing
		private bool isKinematic = false;
		private RigidbodyConstraints originalConstraints;
		private Transform originalParent;

#pragma warning disable IDE0052 // Remove unread private members
		private bool IsKinematic {
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
#pragma warning restore IDE0052 // Remove unread private members

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
		protected void OnCollisionEnter(Collision _) {
			var protagonist = Protagonist.instance;
			if(protagonist.GrabbingObject == this)
				protagonist.GrabbingObject = null;
		}
		#endregion
	}
}