using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Stencil {
	[RequireComponent(typeof(Collider))]
	public class Grabbable : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private UnityEvent onGrabBegin;
		[SerializeField] private UnityEvent onGrabEnd;
		#endregion

		#region Fields
		private new Rigidbody rigidbody;
		private bool wasKinematicWhenGrabbingStarted = false;
		private bool isKinematic = false;
		private RigidbodyConstraints originalConstraints;
		private Transform originalParent;
		#endregion

		#region Interfaces
		public bool IsGrabbed => GameManager.Instance.Protagonist.GrabbingObject == transform;

		public void Grab() {
			GameManager.Instance.Protagonist.GrabbingObject = transform;
		}

		public void Drop() {
			if(IsGrabbed)
				GameManager.Instance.Protagonist.GrabbingObject = null;
		}
		#endregion

		#region Functions
		protected Rigidbody Rigidbody {
			get {
				if(rigidbody != null && rigidbody.transform == transform)
					return rigidbody;
				return rigidbody = GetComponent<Rigidbody>();
			}
		}

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
		#endregion

		#region Message handlers
		protected void OnGrabBegin() {
			wasKinematicWhenGrabbingStarted = IsKinematic;
			IsKinematic = true;
			onGrabBegin.Invoke();
		}

		protected void OnGrabEnd() {
			if(!wasKinematicWhenGrabbingStarted)
				IsKinematic = false;
			onGrabEnd.Invoke();
		}
		#endregion

		#region Life cycle
		protected void Start() {
			wasKinematicWhenGrabbingStarted = IsKinematic;

			var loopshape = transform.EnsureComponent<Loopshape>();
			loopshape.onOpen.AddListener(Grab);
			transform.EnsureComponent<GrabbableValidator>();
		}

		protected void OnCollisionEnter(Collision _) {
			Drop();
		}
		#endregion
	}
}