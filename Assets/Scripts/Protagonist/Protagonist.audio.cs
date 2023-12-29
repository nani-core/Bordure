using UnityEngine;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Fields
		[SerializeField] private AudioSource sfxAudioSource;
		[SerializeField] private AudioSource footAudioSource;
		#endregion

		#region Functions
		public void PlayFootstepSound() {
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