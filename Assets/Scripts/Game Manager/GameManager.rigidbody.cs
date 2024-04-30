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

			float energy = CalculateCollisionSoundEnergy(collision);
			float volume = energy * Settings.audio.collisionSoundGain;
			Vector3 point = FindAverageCollisionPoint(collision);

			PlayCollisionSound(collision.collider, volume, point);
		}

		// TODO: Implement a more reasonable algorithm.
		private float CalculateCollisionSoundEnergy(Collision collision) {
			if(collision == null)
				return default;

			float impulse = collision.impulse.magnitude;
			float minImpulse = Settings.audio.minCollisionImpulse;
			if(impulse < minImpulse)
				return default;
			float hardness = impulse - minImpulse;
			return hardness;
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