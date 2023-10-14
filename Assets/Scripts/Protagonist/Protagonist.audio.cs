using UnityEngine;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[Header("Sound")]
		[SerializeField] private AudioSource sfxAudioSource;
		[SerializeField] private AudioSource footAudioSource;
		#endregion

		#region Functions
		private void PlayFootstepSound() {
			footAudioSource.PlayOneShot(profile.stepAudioClips.PickRandom());
		}

		private void PlaySfx(AudioClip clip) {
			if(clip == null)
				return;
			sfxAudioSource.PlayOneShot(clip);
		}
		#endregion
	}
}