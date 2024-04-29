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
			if(collision?.collider == null)
				return;

			float impulse = collision.impulse.magnitude;
			float minImpulse = Settings.audio.minPhysicalSoundImpulse;
			if(impulse < minImpulse)
				return;
			float hardness = impulse - minImpulse;

			Vector3 point = Vector3.zero;
			foreach(var contact in collision.contacts)
				point += contact.point;
			point /= collision.contacts.Length;

			PlayPhysicalSound(collision.collider, hardness, point);
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