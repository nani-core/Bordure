using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
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

		// Overwritten fields
		private RigidbodyConstraints originalConstraints;
		private Transform originalParent;
		private CollisionDetectionMode originalCollisionDetectionMode;
		private int originalLayer;
		#endregion

		#region Interfaces
		public bool IsGrabbed => GameManager.Instance?.Protagonist?.GrabbingObject == transform;

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
					// Fetch the fields from the rigid body.
					originalConstraints = Rigidbody.constraints;
					originalParent = transform.parent;
					originalCollisionDetectionMode = Rigidbody.collisionDetectionMode;
					originalLayer = gameObject.layer;

					// Overwrite the fields.
					Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					transform.SetParent(null);
					Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
					gameObject.layer = GameManager.Instance.GrabbedLayer;
				}
				else {
					// Restore the fields in the rigid body.
					Rigidbody.constraints = originalConstraints;
					transform.SetParent(originalParent);
					Rigidbody.collisionDetectionMode = originalCollisionDetectionMode;
					gameObject.layer = originalLayer;

					// Write in default values.
					originalConstraints = RigidbodyConstraints.None;
					originalParent = null;
					originalCollisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
					originalLayer = GameManager.Instance.DefaultLayer;
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

		protected void OnDisable() {
			Drop();
		}
		#endregion
	}
}