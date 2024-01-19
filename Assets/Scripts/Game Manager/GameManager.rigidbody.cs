using Palmmedia.ReportGenerator.Core;
using UnityEngine;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Functions
		private static void OnCollisionEnterCallback(Collision collision) {
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
			if(!FlipAgentPairByTier(ref a, ref b))
				return;

			OnRigidbodyCollided(a, b, collision);
		}

		private static bool FlipAgentPairByTier(ref RigidbodyAgent a, ref RigidbodyAgent b) {
			var (tierA, tierB) = (a.GetTier(), b.GetTier());
			if((tierA | tierB) == 0)
				return false;
			if(tierA == 0)
				(a, b) = (b, a);
			return true;
		}

		private static void OnRigidbodyCollided(RigidbodyAgent a, RigidbodyAgent b, Collision collision) {
			float impulse = collision.impulse.magnitude;
			float minImpulse = Instance.Settings.minPhysicalSoundImpulse;
			if(impulse < minImpulse)
				return;
			float hardness = impulse - minImpulse;
			Vector3 point = Vector3.zero;
			foreach(var contact in collision.contacts)
				point += contact.point;
			point /= collision.contacts.Length;
			Instance.PlayPhysicalSound(Instance.Settings.collisionSound, point, a.transform, hardness);
		}

		private static void OnTriggerEnterCallback(Collider trigger, Rigidbody rigidbody) {
			if(trigger.gameObject.layer != Instance.WaterLayer)
				return;

			Instance.PlayPhysicalSound(Instance.Settings.enterWaterSound, rigidbody);
		}

		private static void OnTriggerExitCallback(Collider trigger, Rigidbody rigidbody) {
			if(trigger.gameObject.layer != Instance.WaterLayer)
				return;

			Instance.PlayPhysicalSound(Instance.Settings.exitWaterSound, rigidbody);
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