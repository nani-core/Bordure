using UnityEngine;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(Collider))]
	public abstract class Interactable : MonoBehaviour {
		#region Fields
		private new Rigidbody rigidbody;
		#endregion

		#region Properties
		protected Rigidbody Rigidbody {
			get {
				if(rigidbody != null && rigidbody.transform == transform)
					return rigidbody;
				return rigidbody = GetComponent<Rigidbody>();
			}
		}
		#endregion

		#region Message handlers
		protected virtual void OnFocusEnter() { }

		protected virtual void OnFocusLeave() { }

		protected virtual void OnInteract() { }
		#endregion

		#region Life cycle
		// Dummy handler to make every child class disablable(?).
		protected void Start() { }
		#endregion
	}
}