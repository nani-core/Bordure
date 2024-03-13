using UnityEngine;

namespace NaniCore.Stencil {
	[CreateAssetMenu(menuName = "Nani Core/Game Settings")]
	public class GameSettings : ScriptableObject {
		[Header("Protagonist")]
		public Protagonist protagonist;
		public ProtagonistProfile protagonistProfile;

		[Header("Audio")]
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

		[Header("Water")]
		public Material waterStreamMaterial;
		public Vector3 defaultWaterEjectionVelocity = Vector3.forward;

		[Header("Highlight")]
		public Color defaultHighlightColor = Color.white;
		[Min(0)] public float defaultHighlightDuration = 1.0f;
		public Material defaultHighlightMaterial;
		public Material highlightOffMaterial;
	}
}