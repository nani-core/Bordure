using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Interfaces
		public void PlayPhysicalSound(RigidbodyAgent agent, float strength = 1.0f, Vector3? point = null) {
			if(agent == null && point == null)
				return;

			if(agent == null) {
				// Ensure it's a real null instead of an invalidated object.
				agent = null;
			}

			if(point == null) {
				point = agent?.Rigidbody?.worldCenterOfMass;
				point ??= agent?.transform?.position;
				point ??= Protagonist.Eye.position;
			}

			RigidbodyTier tier = agent?.Tier ?? RigidbodyTier.Default;
			var audio = Settings.audio;
			var soundSet = GetSoundsSetByTier(audio.collisionSoundSets, audio.defaultCollisionSounds, tier);
			var sound = soundSet.PickRandom();

#if DEBUG
			if(agent != null)
				Debug.Log($"{agent.name} (tier: {tier}) is making an collision sound.", agent);
#endif
			PlayWorldSound(sound, point.Value, agent?.transform, strength);
		}

		public void PlayPhysicalSound(Collider collider, float strength = 1.0f, Vector3? point = null) {
			if(collider == null)
				return;

			if(collider.transform.TryGetComponent<RigidbodyAgent>(out var agent)) {
				PlayPhysicalSound(agent, strength, point);
				return;
			}

			if(point == null)
				point = collider.transform.position;
			PlayPhysicalSound(agent, strength, point);
		}

		public void PlayWorldSound(AudioClip sound, Transform transform, float strength = 1.0f) {
			PlayWorldSound(sound, transform.position, transform.transform, strength);
		}

		public void PlayWorldSound(AudioClip sound, Vector3 position, Transform under = null, float strength = 1.0f) {
			if(sound == null)
				return;

			float maxGain = Settings.audio.maxPhysicalSoundGain;
			float volume = (1f - 1f / (strength / maxGain + 1f)) * maxGain;
			volume *= Settings.audio.physicalSoundBaseGain;

			// 这b玩意死活调不好，给我整红温了。
			float rangeMin = Settings.audio.physicalSoundRange * Mathf.Pow(strength, 2.0f);
			float rangeMax = Settings.audio.physicalSoundRange * Mathf.Pow(strength, 1.0f) * Mathf.Exp(-Settings.audio.physicalSoundAttenuation);

			var coroutine = AudioUtility.PlayOneShotAtCoroutine(
				sound, position, under,
				new() {
					volume = volume,
					range = new(rangeMin, rangeMax),
					spatialBlend = 1f,
				}
			);
#if DEBUG
			Debug.Log($"Sound \"{sound.name}\" is being played (strength: {strength}).", sound);
#endif
			Instance.StartCoroutine(coroutine);
		}

		public IList<AudioClip> GetSoundsSetByTier(
			IList<AudioSettings.SoundSet> soundSets,
			IList<AudioClip> defaultSet,
			RigidbodyTier tier
		) {
			if(tier == RigidbodyTier.Default)
				return defaultSet;

			int bestIndex = -1;
			RigidbodyTier bestTier = RigidbodyTier.Default;
			for(int i = 0; i < soundSets.Count; ++i) {
				var set = soundSets[i];
				if(!tier.IsDerivedFromTier(set.tier))
					continue;

				// See if this tier is a better match.
				bool isBetter = set.tier > bestTier;
				if(!isBetter)
					continue;

				bestIndex = i;
				bestTier = set.tier;
			}

			if(bestIndex < 0)
				return defaultSet;
			return soundSets[bestIndex].sounds;
		}
		#endregion
	}
}