using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[Header("Sound")]
		[SerializeField] private AudioSource sfxAudioSource;
		[SerializeField] private AudioClip onFocusSound;
		[SerializeField] private AudioClip onGrabSound;
		[SerializeField] private AudioClip onDropSound;
		[SerializeField] private AudioSource footAudioSource;
		[SerializeField] private List<AudioClip> stepAudioClips = new List<AudioClip>();
		#endregion

		#region Functions
		private void PlayFootstepSound() {
			footAudioSource.PlayOneShot(stepAudioClips.PickRandom());
		}

		private void PlaySfx(AudioClip clip) {
			if(clip == null)
				return;
			sfxAudioSource.PlayOneShot(clip);
		}
		#endregion
	}
}