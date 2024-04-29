using UnityEngine;

namespace NaniCore.Bordure {
	public partial class Protagonist : MonoBehaviour {
		#region Fields
		[SerializeField] private AudioSource footAudioSource;
		#endregion

		#region Functions
		public void PlayFootstepSound() {
			// TODO: Detect walking surface tier.
			var sound = GameManager.Instance.Settings.audio.defaultFootstepSounds.PickRandom();
			footAudioSource.PlayOneShot(sound);
		}

		private void PlaySfx(AudioClip clip) {
			if(clip == null)
				return;
			GameManager.Instance.SfxAudioSource.PlayOneShot(clip);
		}
		#endregion
	}
}