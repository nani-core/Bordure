using UnityEngine;
using System.Collections;

namespace NaniCore {
	public static class AudioUtility {
		public struct AudioPlayConfig {
			public FloatRange range;

			public void ApplyOn(AudioSource source) {
				if(source == null)
					return;
				source.minDistance = range.min;
				source.maxDistance = range.max;
			}
		}

		public static AudioPlayConfig defaultAudioPlayConfig = new AudioPlayConfig {
			range = new FloatRange(0, 1),
		};

		public static IEnumerator PlayOneShotAtCoroutine(AudioClip clip, Vector3 worldPosition, Transform under)
			=> PlayOneShotAtCoroutine(clip, worldPosition, under, defaultAudioPlayConfig);
		public static IEnumerator PlayOneShotAtCoroutine(AudioClip clip, Vector3 worldPosition, Transform under, AudioPlayConfig config) {
			if(clip == null)
				yield break;
			GameObject player = new GameObject("One Shot Audio Player");
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