using UnityEngine;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Fields
		private AudioSource sfxAudioSource;
		private AudioSource footAudioSource;
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

		#region Life cycle
		protected void StartAudio() {
			sfxAudioSource = Eye.gameObject.EnsureComponent<AudioSource>();
			sfxAudioSource.playOnAwake = false;
			footAudioSource = gameObject.EnsureComponent<AudioSource>();
			footAudioSource.playOnAwake = false;
		}
		#endregion
	}
}