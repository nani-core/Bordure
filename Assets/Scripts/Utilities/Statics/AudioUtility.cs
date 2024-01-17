using UnityEngine;
using System.Collections;

namespace NaniCore {
	public static class AudioUtility {
		public struct AudioPlayConfig {
			public Vector2 range;
			public float volume;

			public AudioPlayConfig(Vector2 range, float volume) {
				this.range = range;
				this.volume = volume;
			}
			public AudioPlayConfig(AudioPlayConfig config) : this(config.range, config.volume) { }

			public readonly void ApplyOn(AudioSource source) {
				if(source == null)
					return;

				source.volume = volume;

				source.minDistance = range.x;
				source.maxDistance = range.y;
			}
		}

		public static AudioPlayConfig defaultAudioPlayConfig = new() {
			range = new Vector2(0, 1),
		};

		public static IEnumerator PlayOneShotAtCoroutine(AudioClip clip, Vector3 worldPosition, Transform under)
			=> PlayOneShotAtCoroutine(clip, worldPosition, under, defaultAudioPlayConfig);
		public static IEnumerator PlayOneShotAtCoroutine(AudioClip clip, Vector3 worldPosition, Transform under, AudioPlayConfig config) {
			if(clip == null)
				yield break;
			GameObject player = new("One Shot Audio Player");
			player.transform.position = worldPosition;
			player.transform.SetParent(under, true);
			AudioSource source = player.AddComponent<AudioSource>();
			source.playOnAwake = false;
			config.ApplyOn(source);
			source.PlayOneShot(clip);
			yield return new WaitUntil(() => !source.isPlaying);
			Object.Destroy(player);
		}
	}
}