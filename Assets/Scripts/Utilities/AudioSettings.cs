using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	[CreateAssetMenu(menuName = "Nani Core/Audio Settings")]
	public class AudioSettings : ScriptableObject {
		[Header("Physical parameters")]
		[Min(0)] public float physicalSoundBaseGain = .1f;
		[Min(0)] public float maxPhysicalSoundGain = 5f;
		[Min(0)] public float physicalSoundRange = 10f;
		[Min(0)] public float physicalSoundAttenuation = 5f;
		[Min(0)] public float minPhysicalSoundImpulse = 1f;

		[Header("Water")]
		public AudioClip[] enterWaterSounds;
		public AudioClip[] exitWaterSounds;

		[System.Serializable]
		public struct CollisionSoundSet {
			public RigidbodyTier tier;
			[FormerlySerializedAs("audioClips")] public List<AudioClip> sounds;
		}
		[Header("Collision")]
		public List<AudioClip> defaultCollisionSounds;
		public List<CollisionSoundSet> collisionSoundSets = new();

		[Header("Interaction")]
		public AudioClip onFocusSound;
		public AudioClip onGrabSound;
		public AudioClip onDropSound;

		[Header("Footsteps")]
		public List<AudioClip> defaultFootstepSounds;
		public List<CollisionSoundSet> footstepSoundSets = new();
	}
}