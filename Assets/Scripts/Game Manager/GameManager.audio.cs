using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Interfaces
		public void PlayCollisionSound(RigidbodyAgent agent, float energy, Vector3 point) {
			if(agent == null && point == null)
				return;

			if(agent == null) {
				// Ensure it's a real null instead of an invalidated object.
				agent = null;
			}

			RigidbodyTier tier = agent?.Tier ?? RigidbodyTier.Default;
			var audio = Settings.audio;
			var soundSet = GetSoundsSetByTier(audio.collisionSoundSets, audio.defaultCollisionSounds, tier);
			var sound = soundSet.PickRandom();
			if(sound == null)
				return;
			float soundEnergy = sound.CalculateTotalEnergy();
			float theoreticalVolume = energy / soundEnergy;
			float playingVolume = Mathf.Min(theoreticalVolume, audio.maxVolume);

			if(Settings.makeAudioLogs && agent != null) {
				Debug.Log($"{agent.name} (tier: {tier}) is making an collision sound.", agent);
				Debug.Log(
					string.Join(", ", new string[] {
						$"desired energy = {energy}",
						$"actual enery of {sound.name} = {soundEnergy}",
						$"volume = {theoreticalVolume}" + (playingVolume == theoreticalVolume ? "" : " (clippped)")
					}),
					sound
				);
			}
			PlayWorldSound(sound, point, agent?.transform, playingVolume);
		}

		public void PlayCollisionSound(Collider collider, float energy, Vector3 point) {
			if(collider == null)
				return;

			if(collider.transform.TryGetComponent<RigidbodyAgent>(out var agent)) {
				PlayCollisionSound(agent, energy, point);
				return;
			}

			if(point == null)
				point = collider.transform.position;
			if(Settings.makeAudioLogs) {
				Debug.Log($"{collider.name} is making an collision sound.", agent);
			}
			PlayCollisionSound(null as RigidbodyAgent, energy, point);
		}

		public void PlayWorldSound(AudioClip sound, Transform transform, float volume = 1.0f) {
			PlayWorldSound(sound, transform.position, transform.transform, volume);
		}

		public void PlayWorldSound(AudioClip sound, Vector3 position, Transform under = null, float volume = 1.0f) {
			var audio = Settings?.audio;
			if(sound == null || audio == null)
				return;

			float maxRange = Mathf.Pow(audio.rangeExponentialBase, volume * audio.rangeFactorA) * audio.rangeFactorB;
			AudioUtility.AudioPlayConfig config = new() {
				volume = volume,
				spatialBlend = 1.0f,
				rolloffMode = AudioRolloffMode.Linear,
				range = new Vector2(0.0f, maxRange),
			};
			var coroutine = AudioUtility.PlayOneShotAtCoroutine(sound, position, under, config);

			if(Settings.makeAudioLogs) {
				Debug.Log($"Sound \"{sound.name}\" is played (volume: {volume}, range = {maxRange}).", sound);
			}
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