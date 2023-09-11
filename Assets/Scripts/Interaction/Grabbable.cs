using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.UnityPlayground {
	public class Grabbable : Interaction {
		#region Serialized fields
		[SerializeField] private UnityEvent onGrabBegin;
		[SerializeField] private UnityEvent onGrabEnd;
		#endregion

		#region Overridden message handlers
		protected override void OnFocusEnter() {
		}

		protected override void OnFocusLeave() {
		}

		protected override void OnInteract() {
			Protagonist.instance.GrabbingObject = this;
		}
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

		protected void OnGrabBegin() {
			IsKinematic = true;
			onGrabBegin.Invoke();
		}

		protected void OnGrabEnd() {
			IsKinematic = false;
			onGrabEnd.Invoke();
		}

		protected void OnCollisionEnter(Collision _) {
			var protagonist = Protagonist.instance;
			if(protagonist.GrabbingObject == this)
				protagonist.GrabbingObject = null;
		}
		#endregion
	}
}