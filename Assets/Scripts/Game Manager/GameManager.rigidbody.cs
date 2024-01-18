using UnityEngine;

namespace NaniCore.Loopool {
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
				contactPoint.thisCollider.attachedRigidbody?.GetComponent<RigidbodyAgent>(),
				contactPoint.otherCollider.attachedRigidbody?.GetComponent<RigidbodyAgent>()
			);

			var (tierA, tierB) = (a.GetTier(), b.GetTier());
			if((tierA | tierB) == 0)
				return;

			if(tierA == 0) {
				(a, b) = (b, a);
#pragma warning disable IDE0059
				// They will be sooner or later used.
				(tierA, tierB) = (tierB, tierA);
#pragma warning restore IDE0059
			}

			OnRigidbodyCollided(a, b, collision);
		}

		private void OnRigidbodyCollided(RigidbodyAgent a, RigidbodyAgent b, Collision collision) {
			foreach(var contact in collision.contacts) {
				float hardness = collision.impulse.magnitude / (a.Rigidbody.mass * (b?.Rigidbody?.mass ?? 1));
				// Generate collision sounds.
				{
					// TODO: The attenuation coefficient 10f should be configured.
					a.StartCoroutine(AudioUtility.PlayOneShotAtCoroutine(
						// TODO: The collision sound should be regarding the tiers.
						Settings.collisionSound,
						contact.point,
						a.transform,
						new() {
							volume = 1f - 1f / (hardness / 10f + 1f),
						}
					));
				}
			}
		}
		#endregion

		#region Life cycle
		protected void InitializeRigidbody() {
			RigidbodyAgent.onColliderEnterStatic += OnCollisionEnterCallback;
			// TODO: Enter and leave water.
		}
		#endregion
	}
}