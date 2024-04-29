using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		[SerializeField] private AudioListener audioListener;
		[SerializeField] private AudioSource sfxAudioSource;
		#endregion

		#region Interfaces
		public AudioListener AudioListener => audioListener;
		public AudioSource SfxAudioSource => sfxAudioSource;

		public void PlayPhysicalSound(AudioClip sound, Vector3 position, Transform under, float strength) {
			float maxGain = Settings.audio.maxPhysicalSoundGain;
			float volume = (1f - 1f / (strength / maxGain + 1f)) * maxGain;
			volume *= Settings.audio.physicalSoundBaseGain;

			// 这b玩意死活调不好，给我整红温了。
			Vector2 range = Vector2.zero;
			range.y = Settings.audio.physicalSoundRange * strength;
			range.x = range.y * Mathf.Exp(-Settings.audio.physicalSoundAttenuation);
			range.y *= strength;

			var coroutine = AudioUtility.PlayOneShotAtCoroutine(
				sound, position, under,
				new() {
					volume = volume,
					range = range,
					spatialBlend = 1f,
				}
			);
			Instance.StartCoroutine(coroutine);
		}

		public void PlayPhysicalSound(AudioClip sound, Rigidbody rb) {
			float strength = rb.mass;
			if(!rb.isKinematic) {
				if((rb.constraints | RigidbodyConstraints.FreezePosition) == 0)
					strength *= rb.velocity.magnitude;
			}

			PlayPhysicalSound(sound, rb.position, rb.transform, strength);
		}
		#endregion
	}
}