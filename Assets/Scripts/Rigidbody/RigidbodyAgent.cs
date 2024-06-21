using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	public class RigidbodyAgent : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private RigidbodyTier tier;
		#endregion

		#region Fields
		private new Rigidbody rigidbody;
		private readonly Dictionary<Collider, ContactPoint[]> colliderContacts = new();
		private readonly HashSet<Collider> overlappingTriggers = new();
		#endregion

		#region Interfaces
		public Rigidbody Rigidbody => rigidbody;
		public RigidbodyTier Tier {
			get => tier;
			set => tier = value;
		}

		public Dictionary<Collider, ContactPoint[]> ColliderContacts => colliderContacts;
		public HashSet<Collider> OverlappingTriggers => overlappingTriggers;

		public bool IsOverlappingWithLayers(LayerMask layerMask) {
			return overlappingTriggers.Any(trigger => {
				if(trigger == null)
					return false;
				return ((1 << trigger.gameObject.layer) & layerMask) != 0;
			});
		}
		#endregion

		#region Delegates
		#region Instance
		public delegate void ColliderContactUpdateNotifier(Collision collision);
		public ColliderContactUpdateNotifier onColliderEnter;
		public ColliderContactUpdateNotifier onColliderExit;

		public delegate void TriggerUpdateNotifier(Collider collider);
		public TriggerUpdateNotifier onTriggerEnter;
		public TriggerUpdateNotifier onTriggerExit;
		#endregion

		#region Static
		public delegate void StaticColliderContactUpdateNotifier(Collision collision);
		public static StaticColliderContactUpdateNotifier onColliderEnterStatic;
		public static StaticColliderContactUpdateNotifier onColliderExitStatic;

		public delegate void StaticTriggerUpdateNotifier(Collider trigger, Rigidbody rigidbody);
		public static StaticTriggerUpdateNotifier onTriggerEnterStatic;
		public static StaticTriggerUpdateNotifier onTriggerExitStatic;
		#endregion
		#endregion

		#region Functions
		private void UpdateColliderContact(Collision collision) {
			var collider = collision.collider;
			if(collider == null)
				return;
			var contactCount = collision.contactCount;
			if(contactCount == 0) {
				// Remove escaped collider.
				if(colliderContacts.ContainsKey(collider)) {
					colliderContacts.Remove(collider);
					onColliderExit?.Invoke(collision);
					onColliderExitStatic?.Invoke(collision);
				}
				return;
			}

			bool isNew = false;
			ContactPoint[] contactArray;
			if(colliderContacts.ContainsKey(collider)) {
				// Update contacting collider.
				contactArray = colliderContacts[collider];
				if(contactArray.Length < contactCount) {
					var newContactArray = new ContactPoint[contactCount];
					contactArray.CopyTo(newContactArray, 0);
					contactArray = newContactArray;
				}
			}
			else {
				// Record new contacting collider.
				colliderContacts.Add(collider, null);
				contactArray = new ContactPoint[contactCount];
				isNew = true;
			}

			collision.GetContacts(contactArray);
			colliderContacts[collider] = contactArray;
			if(isNew) {
				onColliderEnter?.Invoke(collision);
				onColliderEnterStatic?.Invoke(collision);
			}
		}
		#endregion

		#region Life cycle
		protected void Start() {
			if(!TryGetComponent(out rigidbody))
				rigidbody = null;
		}

		protected void OnCollisionEnter(Collision collision) => UpdateColliderContact(collision);
		protected void OnCollisionStay(Collision collision) => UpdateColliderContact(collision);
		protected void OnCollisionExit(Collision collision) => UpdateColliderContact(collision);

		protected void OnTriggerEnter(Collider trigger) {
			if(trigger == null)
				return;
			overlappingTriggers.Add(trigger);
			onTriggerEnter?.Invoke(trigger);
			onTriggerEnterStatic?.Invoke(trigger, Rigidbody);
		}
		protected void OnTriggerExit(Collider trigger) {
			if(trigger == null)
				return;
			overlappingTriggers.Remove(trigger);
			onTriggerExit?.Invoke(trigger);
			onTriggerExitStatic?.Invoke(trigger, Rigidbody);
		}

		protected void FixedUpdate() {
			// Remove all invalidated targets.

			var collidersToBeRemoved = colliderContacts.Keys
				.Where(collider => collider == null)
				.ToArray();
			foreach(var removee in collidersToBeRemoved)
				colliderContacts.Remove(removee);

			overlappingTriggers.RemoveWhere(trigger => trigger == null);
		}
		#endregion
	}
}
