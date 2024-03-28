using UnityEngine;

namespace NaniCore.Bordure {
	public partial class Protagonist : MonoBehaviour {
		#region Fields
		[SerializeField] private AudioListener audioListener;
		[SerializeField] private AudioSource sfxAudioSource;
		[SerializeField] private AudioSource footAudioSource;
		#endregion

		#region Functions
		public void PlayFootstepSound() {
			footAudioSource.PlayOneShot(Profile.stepAudioClips.PickRandom());
		}

		private void PlaySfx(AudioClip clip) {
			if(clip == null)
				return;
			sfxAudioSource.PlayOneShot(clip);
		}
		#endregion

		#region Life cycle
		protected void InitializeAudio() {
			GameManager.Instance.AudioListener = audioListener;
		}
		#endregion
	}
}