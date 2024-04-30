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
		[Min(0)] public float collisionSoundGain = 1f;
		[Min(0)] public float minCollisionImpulse = 0.1f;
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