using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Functions
		private void OnCollisionEnterCallback(Collision collision) {
			if(collision == null || collision.contactCount == 0)
				return;

			OnBodiesCollided(collision);
		}

		private void OnBodiesCollided(Collision collision) {
			if(collision == null)
				return;

			float fullEnergy = CalculateCollisionSoundEnergy(collision);
			float energy = fullEnergy * Settings.audio.CollisionSoundEnergyConversionRate;
			Vector3 point = FindAverageCollisionPoint(collision);

			PlayCollisionSound(collision.contacts[0].thisCollider, energy, point);
		}

		private float CalculateCollisionSoundEnergy(Collision collision) {
			if(collision == null)
				return default;

			Collider a = collision.contacts[0].thisCollider, b = collision.contacts[0].otherCollider;
			float ma = GetEquivalentMass(a), mb = GetEquivalentMass(b);

			float totalEnergy = Mathf.Pow(collision.impulse.magnitude, 2) * (1 / ma + 1 / mb) * 0.5f;

			return totalEnergy * 0.5f;
		}

		private float GetEquivalentMass(Collider collider) {
			if(collider == null)
				return Mathf.Infinity;
			if(!collider.TryGetComponent<Rigidbody>(out var rb))
				return Mathf.Infinity;
			if(rb.isKinematic)
				return Mathf.Infinity;
			if(rb.constraints == RigidbodyConstraints.FreezePosition)
				return Mathf.Infinity;
			return rb.mass;
		}

		private Vector3 FindAverageCollisionPoint(Collision collision) {
			if(collision == null)
				return default;

			Vector3 point = Vector3.zero;
			foreach(var contact in collision.contacts)
				point += contact.point;
			point /= collision.contacts.Length;

			return point;
		}

		private void OnTriggerEnterCallback(Collider trigger, Rigidbody rigidbody) {
			if(trigger.gameObject.layer != WaterLayer)
				return;

			PlayWorldSound(Settings.audio.enterWaterSounds.PickRandom(), rigidbody.transform);
		}

		private void OnTriggerExitCallback(Collider trigger, Rigidbody rigidbody) {
			if(trigger.gameObject.layer != WaterLayer)
				return;

			PlayWorldSound(Settings.audio.exitWaterSounds.PickRandom(), rigidbody.transform);
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