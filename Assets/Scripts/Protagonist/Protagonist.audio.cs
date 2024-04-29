using UnityEngine;

namespace NaniCore.Bordure {
	public partial class Protagonist : MonoBehaviour {
		#region Functions
		public void PlayFootstepSound() {
			// TODO: Detect walking surface tier.
			var sound = GameManager.Instance.Settings.audio.defaultFootstepSounds.PickRandom();
			GameManager.Instance.PlayWorldSound(sound, foot.position);
		}

		private void PlaySfx(AudioClip clip) {
			if(clip == null)
				return;
			GameManager.Instance.PlayWorldSound(clip, Eye);
		}
		#endregion
	}
}