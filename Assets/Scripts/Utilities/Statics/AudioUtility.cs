using UnityEngine;
using System.Collections;

namespace NaniCore {
	public static class AudioUtility {
		public struct AudioPlayConfig {
			public Vector2 range;
			public float volume;
			public float spatialBlend;
			public AudioRolloffMode rolloffMode;

			public readonly void ApplyOn(AudioSource source) {
				if(source == null)
					return;

				source.volume = volume;

				source.minDistance = range.x;
				source.maxDistance = range.y;
				source.spatialBlend = spatialBlend;
				source.rolloffMode = rolloffMode;
			}
		}

		public static IEnumerator PlayOneShotAtCoroutine(AudioClip clip, Vector3 worldPosition, Transform under, AudioPlayConfig config) {
			if(clip == null)
				yield break;

			config.volume = Mathf.Abs(config.volume);
			if(config.volume == 0.0f) {
				yield return new WaitForSeconds(clip.length);
				yield break;
			}
			bool needToGainClipAtRuntime = config.volume > 1.0f;
			// If volume is greater than 1.0f, we need to create a level-gained version of the clip at runtime.
			if(needToGainClipAtRuntime) {
				var copy = AudioClip.Create($"{clip.name} (gained)", clip.samples, clip.channels, clip.frequency, false);
				Bordure.GameManager.Instance.RegisterTemporaryResource(copy);
				float[] data = new float[clip.samples];
				try {
					clip.GetData(data, 0);
				}
				catch(System.Exception err) {
					Debug.LogWarning($"Warning: Cannot get data of audio clip {clip}, aborting sound playing.", clip);
					Debug.LogError(err);
					yield break;
				}
				for(int i = 0; i < data.Length; ++i)
					data[i] *= config.volume;
				config.volume = 1.0f;
				copy.SetData(data, 0);
				clip = copy;
			}

			GameObject player = new($"Audio ({clip.name})");
			player.transform.position = worldPosition;
			player.transform.SetParent(under, true);

			AudioSource source = player.AddComponent<AudioSource>();
			source.playOnAwake = false;
			config.ApplyOn(source);

			source.PlayOneShot(clip);

			// The AudioSource instance might be destroyed during playing.
			yield return new WaitUntil(() => {
				if(source == null)
					return true;
				return !source.isPlaying;
			});

			if(player != null)
				Object.Destroy(player);
			if(needToGainClipAtRuntime) {
				Bordure.GameManager.Instance.ReleaseResource(clip);
			}
		}
		public static IEnumerator PlayOneShotAtCoroutine(AudioClip clip, Vector3 worldPosition, Transform under) {
			AudioPlayConfig config = new() {
				range = new Vector2(1.0f, 500.0f),
				volume = 1.0f,
				spatialBlend = 1.0f,
				rolloffMode = AudioRolloffMode.Logarithmic,
			};
			return PlayOneShotAtCoroutine(clip, worldPosition, under, config);
		}

		public static float CalculateTotalEnergy(this AudioClip clip) {
			if(clip == null)
				return default;

			float[] samples = new float[clip.samples];
			try {
				clip.GetData(samples, 0);
			}
			catch(System.Exception err) {
				Debug.LogWarning($"Cannot calculate the total energy of {clip} as its data cannot be read.");
				Debug.LogError(err);
				return default;
			}

			float sum = 0.0f;
			float timeFactor = Mathf.Pow(1.0f / clip.frequency, 1);
			for(int i = 0; i < samples.Length; ++i) {
				float sample = Mathf.Abs(samples[i]);
				sum += sample * timeFactor;
			}

			return sum;
		}
	}
}