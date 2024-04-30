using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	[CreateAssetMenu(menuName = "Nani Core/Audio Settings")]
	public class AudioSettings : ScriptableObject {
		[Header("Water")]
		public AudioClip[] enterWaterSounds;
		public AudioClip[] exitWaterSounds;

		[System.Serializable]
		public struct SoundSet {
			public RigidbodyTier tier;
			public List<AudioClip> sounds;
		}
		[Header("Collision")]
		[Tooltip("How much portion of the enery lost due to collision will be transfered into sound. Measured in logarithm in 10.")]
		[NaughtyAttributes.Label("Energy Conversion Rate (log 10)")]
		[Range(-10, 0)][SerializeField] private float collisionSoundEnergyConversionRateLog = -3f;
		public float CollisionSoundEnergyConversionRate => Mathf.Pow(10, collisionSoundEnergyConversionRateLog);
		[Tooltip("How strongly can collision sounds be played at maximum. This is to prevent a too-loud sound from being played.")]
		[Range(0, 10)] public float maxVolume = 1.0f;
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