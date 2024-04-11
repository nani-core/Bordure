using UnityEngine;

namespace NaniCore.Bordure {
	public partial class Protagonist : MonoBehaviour {
		#region Fields
		[SerializeField] private AudioSource footAudioSource;
		#endregion

		#region Functions
		public void PlayFootstepSound() {
			footAudioSource.PlayOneShot(Profile.stepAudioClips.PickRandom());
		}

		private void PlaySfx(AudioClip clip) {
			if(clip == null)
				return;
			GameManager.Instance.SfxAudioSource.PlayOneShot(clip);
		}
		#endregion
	}
}