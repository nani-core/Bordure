using UnityEngine;
using System.Collections;

namespace NaniCore {
	public static class AudioUtility {
		public struct AudioPlayConfig {
			public Vector2 range;
			public float volume;
			public float spatialBlend;

			public AudioPlayConfig(Vector2 range, float volume, float spatialBlend) {
				this.range = range;
				this.volume = volume;
				this.spatialBlend = spatialBlend;
			}
			public AudioPlayConfig(AudioPlayConfig config) : this(config.range, config.volume, config.spatialBlend) { }

			public readonly void ApplyOn(AudioSource source) {
				if(source == null)
					return;

				source.volume = volume;

				source.minDistance = range.x;
				source.maxDistance = range.y;
				source.spatialBlend = spatialBlend;
			}
		}

		public static AudioPlayConfig defaultAudioPlayConfig = new() {
			range = new Vector2(0, 1),
			volume = 1f,
			spatialBlend = 1f,
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