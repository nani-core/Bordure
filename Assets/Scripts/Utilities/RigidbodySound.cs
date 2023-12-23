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
			var rv = collision.relativeVelocity;
			foreach(var contact in collision.contacts) {
				var collidingSpeed = Vector3.Dot(rv, contact.normal);
				if(collidingSpeed >= minCollideSpeed) {
					if(!inWater)
						PlaySound(onCollide.PickRandom(), contact.point);
				}
			}
		}

		protected void OnTriggerEnter(Collider other) {
			if(other.gameObject.layer == LayerMask.NameToLayer("Water")) {
				inWater = true;
				PlaySound(onEnterWater.PickRandom());
			}
		}

		protected void OnTriggerExit(Collider other) {
			if(other.gameObject.layer == LayerMask.NameToLayer("Water")) {
				inWater = false;
				PlaySound(onLeaveWater.PickRandom());
			}
		}
		#endregion
	}
}