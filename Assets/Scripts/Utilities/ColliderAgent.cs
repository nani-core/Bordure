using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.UnityPlayground {
	public class ColliderAgent : MonoBehaviour {
		public Collider Collider => GetComponent<Collider>();

		public UnityEvent<Collision> onCollisionEnter, onCollisionStay, onCollisionExit;
		public UnityEvent<Collider> onTriggerEnter, onTriggerStay, onTriggerExit;

		protected void OnCollisionEnter(Collision collision) => onCollisionEnter.Invoke(collision);
		protected void OnCollisionStay(Collision collision) => onCollisionStay.Invoke(collision);
		protected void OnCollisionExit(Collision collision) => onCollisionExit.Invoke(collision);

		protected void OnTriggerEnter(Collider collider) => onTriggerEnter.Invoke(collider);
		protected void OnTriggerStay(Collider collider) => onTriggerStay.Invoke(collider);
		protected void OnTriggerExit(Collider collider) => onTriggerExit.Invoke(collider);
	}
}