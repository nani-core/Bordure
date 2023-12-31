using UnityEngine;
using System.Collections;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(Rigidbody))]
	public class RigidbodySound : MonoBehaviour {
		#region Serialized fields
		[Min(0)] public float minCollideSpeed = 2f;
		public AudioClip[] onCollide;
		public AudioClip[] onEnterWater;
		public AudioClip[] onLeaveWater;
		#endregion

		#region Fields
		private bool immune = true;
		private bool inWater = false;
		#endregion

		#region Functions
		public void PlaySound(AudioClip clip)
			=> PlaySound(clip, transform.position);
		public void PlaySound(AudioClip clip, Vector3 worldPosition, float volume = 1f) {
			if(clip == null)
				return;

			AudioUtility.AudioPlayConfig audioConfig = new(AudioUtility.defaultAudioPlayConfig) {
				volume = volume,
			};
			StartCoroutine(AudioUtility.PlayOneShotAtCoroutine(clip, worldPosition, transform, audioConfig));
		}

		private IEnumerator ImmuneCoroutine(float time = .1f) {
			immune = true;
			yield return new WaitForSeconds(time);
			immune = false;
		}
		#endregion

		#region Life cycle
		protected void OnEnable() {
			StartCoroutine(ImmuneCoroutine());
		}

		protected void OnCollisionEnter(Collision collision) {
			if(immune || inWater)
				return;

			var rv = collision.relativeVelocity;
			foreach(var contact in collision.contacts) {
				var collidingSpeed = Vector3.Dot(rv, contact.normal);
				if(collidingSpeed >= minCollideSpeed) {
					var clip = onCollide.PickRandom();
					float volume = 1 - minCollideSpeed / collidingSpeed;
					Debug.Log($"Playing {clip} with volume={volume}.");
					PlaySound(clip, contact.point, volume);
				}
			}
		}

		protected void OnTriggerEnter(Collider other) {
			if(immune)
				return;

			if(!inWater) {
				if(other.gameObject.layer == LayerMask.NameToLayer("Water")) {
					inWater = true;
					PlaySound(onEnterWater.PickRandom());
				}
			}
		}

		protected void OnTriggerExit(Collider other) {
			if(immune)
				return;

			if(inWater) {
				if(other.gameObject.layer == GameManager.Instance.WaterLayer) {
					inWater = false;
					PlaySound(onLeaveWater.PickRandom());
				}
			}
		}
		#endregion
	}
}