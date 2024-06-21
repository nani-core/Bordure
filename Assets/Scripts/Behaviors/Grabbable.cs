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
		private Vector3 initialPosition;
		private Quaternion initialRotation;
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

		#region Message handlers
		protected void OnGrabBegin() {
			onGrabBegin.Invoke();
		}

		protected void OnGrabEnd() {
			onGrabEnd.Invoke();
		}
		#endregion

		#region Life cycle
		protected void Start() {
			var loopshape = transform.EnsureComponent<Loopshape>();
			loopshape.onOpen.AddListener(Grab);
			transform.EnsureComponent<GrabbableValidator>();

			initialPosition = transform.position;
			initialRotation = transform.rotation;

			StartCoroutine(CheckForDeathHeight());
		}

		protected void OnDisable() {
			Drop();
		}
		#endregion

		#region Functions

		protected System.Collections.IEnumerator CheckForDeathHeight() {
			while(true) {
				yield return new WaitForSeconds(2f);
				if(GameManager.Instance == null)
					continue;
				if(transform.position.y < GameManager.Instance.Settings.deathHeight) {
					transform.position = initialPosition;
					transform.rotation = initialRotation;
					if(TryGetComponent(out Rigidbody rb)) {
						rb.velocity = default;
						rb.angularVelocity = default;
					}
					Debug.LogWarning($"Warning: {name} has fallen below death height and is put back.", this);
				}
			}
		}
		#endregion
	}
}