using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Interfaces
		public void PlayCollisionSound(RigidbodyAgent agent, float volume = 1.0f, Vector3? point = null) {
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

			if(sound == null)
				return;
#if DEBUG
			if(agent != null)
				Debug.Log($"{agent.name} (tier: {tier}) is making an collision sound.", agent);
#endif

			PlayWorldSound(sound, point.Value, agent?.transform, volume);
		}

		public void PlayCollisionSound(Collider collider, float volume = 1.0f, Vector3? point = null) {
			if(collider == null)
				return;

			if(collider.transform.TryGetComponent<RigidbodyAgent>(out var agent)) {
				PlayCollisionSound(agent, volume, point);
				return;
			}

			if(point == null)
				point = collider.transform.position;
#if DEBUG
			Debug.Log($"{collider.name} is making an collision sound.", agent);
#endif
			PlayCollisionSound(null as RigidbodyAgent, volume, point);
		}

		public void PlayWorldSound(AudioClip sound, Transform transform, float volume = 1.0f) {
			PlayWorldSound(sound, transform.position, transform.transform, volume);
		}

		public void PlayWorldSound(AudioClip sound, Vector3 position, Transform under = null, float volume = 1.0f) {
			if(sound == null)
				return;

			var coroutine = AudioUtility.PlayOneShotAtCoroutine(sound, position, under, volume);
#if DEBUG
			Debug.Log($"Sound \"{sound.name}\" is played (volume: {volume}).", sound);
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