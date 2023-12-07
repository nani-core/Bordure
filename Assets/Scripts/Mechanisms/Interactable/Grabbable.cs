using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Loopool {
	public class Grabbable : Interactable {
		#region Serialized fields
		[SerializeField] private UnityEvent onGrabBegin;
		[SerializeField] private UnityEvent onGrabEnd;
		#endregion

		#region Message handlers
		protected override void OnInteract() {
			if(!isActiveAndEnabled)
				return;
			GameManager.Instance.Protagonist.GrabbingObject = this;
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
				if(Rigidbody == null)
					return;
				if(value == isKinematic)
					return;
				if(value) {
					originalConstraints = Rigidbody.constraints;
					originalParent = transform.parent;

					Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					transform.SetParent(null);
				}
				else {
					Rigidbody.constraints = originalConstraints;
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
			var protagonist = GameManager.Instance?.Protagonist;
			if(protagonist == null)
				return;
			if(protagonist.GrabbingObject == this)
				protagonist.GrabbingObject = null;
		}
		#endregion
	}
}