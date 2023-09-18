using UnityEngine;

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
		private bool inWater = false;
		#endregion

		#region Functions
		public void PlaySound(AudioClip clip)
			=> PlaySound(clip, transform.position);
		public void PlaySound(AudioClip clip, Vector3 worldPosition) {
			if(clip == null)
				return;
			StartCoroutine(AudioUtility.PlayOneShotAtCoroutine(clip, worldPosition, transform));
		}
		#endregion

		#region Life cycle
		protected void OnCollisionEnter(Collision collision) {
			if(collision.relativeVelocity.magnitude >= minCollideSpeed) {
				if(!inWater)
					PlaySound(onCollide.PickRandom(), collision.contacts[0].point);
			}
		}

		protected void OnTriggerEnter(Collider other) {
			if(other.GetComponent<Water>()) {
				inWater = true;
				PlaySound(onEnterWater.PickRandom());
			}
		}

		protected void OnTriggerExit(Collider other) {
			if(other.GetComponent<Water>()) {
				inWater = false;
				PlaySound(onLeaveWater.PickRandom());
			}
		}
		#endregion
	}
}