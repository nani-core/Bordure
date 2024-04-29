using UnityEngine;

namespace NaniCore.Bordure {
	[CreateAssetMenu(menuName = "Nani Core/Audio Settings")]
	public class AudioSettings : ScriptableObject {
		[Header("Physical parameters")]
		[Min(0)] public float physicalSoundBaseGain = .1f;
		[Min(0)] public float maxPhysicalSoundGain = 5f;
		[Min(0)] public float physicalSoundRange = 10f;
		[Min(0)] public float physicalSoundAttenuation = 5f;
		[Min(0)] public float minPhysicalSoundImpulse = 1f;

		[Header("Rigidbody")]
		// Only temporary.
		public AudioClip collisionSound;
		public AudioClip enterWaterSound;
		public AudioClip exitWaterSound;
	}
}