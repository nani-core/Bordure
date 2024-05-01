using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	[CreateAssetMenu(menuName = "Nani Core/Audio Settings")]
	public class AudioSettings : ScriptableObject {
		[Header("Water")]
		[Range(-10, 0)][SerializeField] private float waterSoundEnergyConversionRateLog10 = -3.0f;
		public float WaterSoundEnergyConversionRate => Mathf.Pow(10, waterSoundEnergyConversionRateLog10);
		public AudioClip[] enterWaterSounds;
		public AudioClip[] exitWaterSounds;

		[System.Serializable]
		public struct SoundSet {
			public RigidbodyTier tier;
			public List<AudioClip> sounds;
		}
		[Header("Collision")]
		[Tooltip("How much portion of the enery lost due to collision will be transfered into sound. Measured in logarithm in 10.")]
		[Range(-10, 0)][SerializeField] private float collisionSoundEnergyConversionRateLog10 = -3.0f;
		public float CollisionSoundEnergyConversionRate => Mathf.Pow(10, collisionSoundEnergyConversionRateLog10);
		[Range(0, 10)] public float rangeFactorA = 2.0f;
		[Range(1, 5)] public float rangeExponentialBase = 3.0f;
		[Range(0, 10)] public float rangeFactorB = 3.0f;
		[Tooltip("How strongly can collision sounds be played at maximum. This is to prevent a too-loud sound from being played.")]
		[Range(0, 3)] public float maxVolume = 1.0f;
		public List<AudioClip> defaultCollisionSounds;
		public List<SoundSet> collisionSoundSets = new();

		[Header("Grabbing")]
		public AudioClip onFocusSound;
		public AudioClip onGrabSound;
		public AudioClip onDropSound;

		[Header("Footsteps")]
		public List<AudioClip> defaultFootstepSounds;
		public List<SoundSet> footstepSoundSets = new();

		[Header("Loopshape")]
		public AudioClip onValidatedSound;
		public AudioClip onOpticalValidatedSound;
		public AudioClip onInvalidatedSound;
		public AudioClip onOpticalInvalidatedSound;
		public AudioClip onOpenedSound;
	}
}