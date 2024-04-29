using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		[SerializeField] private AudioListener audioListener;
		[SerializeField] private AudioSource sfxAudioSource;
		#endregion

		#region Interfaces
		public AudioListener AudioListener => audioListener;
		public AudioSource SfxAudioSource => sfxAudioSource;

		public void PlayPhysicalSound(Rigidbody rb, float strength = 1.0f, Vector3? point = null) {
			if(rb == null)
				return;

			if(point == null)
				point = rb.worldCenterOfMass;

			RigidbodyTier tier = rb.GetTier();
			var sound = GetCollisionSoundsByTier(tier).PickRandom();

#if DEBUG
			Debug.Log($"{rb.name} (tier: {tier}) is making an collision sound.", rb);
#endif
			PlayWorldSound(sound, point.Value, rb.transform, strength);
		}

		public void PlayWorldSound(AudioClip sound, Transform transform, float strength = 1.0f) {
			PlayWorldSound(sound, transform.position, transform.transform, strength);
		}

		public void PlayWorldSound(AudioClip sound, Vector3 position, Transform under, float strength) {
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
		#endregion

		#region Functions
		private IList<AudioClip> GetCollisionSoundsByTier(RigidbodyTier tier) {
			if(Settings.audio == null)
				return default;

			if(tier == RigidbodyTier.Default)
				return Settings.audio.defaultCollisionSounds;

			var sets = Settings.audio.collisionSoundSets;
			int bestIndex = -1;
			RigidbodyTier bestTier = RigidbodyTier.Default;
			for(int i = 0; i < sets.Count; ++i) {
				var set = sets[i];
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
				return Settings.audio.defaultCollisionSounds;
			return sets[bestIndex].audioClips;
		}
		#endregion
	}
}