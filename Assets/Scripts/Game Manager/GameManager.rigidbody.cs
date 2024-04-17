using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Functions
		private void OnCollisionEnterCallback(Collision collision) {
			if(collision == null || collision.contactCount == 0)
				return;

			ContactPoint contactPoint;
			{
				ContactPoint[] contactPoints = new ContactPoint[1];
				collision.GetContacts(contactPoints);
				contactPoint = contactPoints[0];
			}
			var (a, b) = (
				contactPoint.thisCollider.GetComponent<RigidbodyAgent>(),
				contactPoint.otherCollider.GetComponent<RigidbodyAgent>()
			);

			OnRigidbodyCollided(a, b, collision);
		}

		private void OnRigidbodyCollided(RigidbodyAgent a, RigidbodyAgent b, Collision collision) {
			if(a == null) {
				Debug.LogWarning("Warning: Cannot play the requested collision sound as all RB agents are null.");
				return;
			}

			float impulse = collision.impulse.magnitude;
			float minImpulse = Settings.minPhysicalSoundImpulse;
			if(impulse < minImpulse)
				return;
			float hardness = impulse - minImpulse;

			Vector3 point = Vector3.zero;
			foreach(var contact in collision.contacts)
				point += contact.point;
			point /= collision.contacts.Length;

			PlayPhysicalSound(Settings.collisionSound, point, a.transform, hardness);
		}

		private void OnTriggerEnterCallback(Collider trigger, Rigidbody rigidbody) {
			if(trigger.gameObject.layer != WaterLayer)
				return;

			PlayPhysicalSound(Settings.enterWaterSound, rigidbody);
		}

		private void OnTriggerExitCallback(Collider trigger, Rigidbody rigidbody) {
			if(trigger.gameObject.layer != WaterLayer)
				return;

			PlayPhysicalSound(Settings.exitWaterSound, rigidbody);
		}
		#endregion

		#region Life cycle
		protected void InitializeRigidbody() {
			RigidbodyAgent.onColliderEnterStatic += OnCollisionEnterCallback;
			RigidbodyAgent.onTriggerEnterStatic += OnTriggerEnterCallback;
			RigidbodyAgent.onTriggerExitStatic += OnTriggerExitCallback;
		}
		#endregion
	}
}