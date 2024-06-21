using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class Protagonist : MonoBehaviour {
		#region Functions
		public void PlayFootstepSound() {
			var ground = steppingGround.collider;
			if(ground == null)
				return;
			var audio = GameManager.Instance.Settings.audio;
			IList<AudioClip> sounds;
			ground.TryGetComponent<RigidbodyAgent>(out var agent);
			RigidbodyTier tier = agent == null ? RigidbodyTier.Default : agent.Tier;
			sounds = GameManager.Instance.GetSoundsSetByTier(audio.footstepSoundSets, audio.defaultFootstepSounds, tier);
#if DEBUG
			if(GameManager.Instance.Settings.makeAudioLogs) {
				Debug.Log($"Making footstep sound on {ground.name} (tier: {tier}).", ground);
			}
#endif
			GameManager.Instance.PlayWorldSound(sounds.PickRandom(), foot.position, null, 1.0f);
		}

		private void PlaySfx(AudioClip clip) {
			if(clip == null)
				return;
			GameManager.Instance.PlayWorldSound(clip, Eye);
		}
		#endregion
	}
}