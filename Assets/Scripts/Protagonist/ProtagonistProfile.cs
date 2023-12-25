using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Loopool {
	[CreateAssetMenu(menuName = "Nani Core/Loopool/Protagonist Profile")]
	public class ProtagonistProfile : ScriptableObject {
		[Header("Geometry")]
		[Min(0)] public float height = 1.6f;
		[Min(0)] public float radius = .3f;
		[Min(0)] public float eyeHanging = .1f;

		[Header("Control")]
		[Min(0)] public float skinDepth = .08f;
		[Min(0)] public float walkingSpeed = 3f;
		[Min(0)] public float sprintingSpeed = 5f;
		[Range(0, 1)] public float acceleration = .5f;
		[Min(0)] public float stepDistance = 1.3f;
		[Min(0)] public float orientingSpeed = 1f;
		[Min(0)] public float jumpingHeight = 1f;
		[Min(0)] public float stepHeight = .3f;
		[Tooltip("How will the control ability decrease when midair.")]
		[Range(0, 1)] public float midAirAttenuation = .5f;

		[Header("Interaction")]
		[Min(0)] public float maxInteractionDistance = 20f;
		[Range(0, 1)] public float grabbingTransitionDuration = .2f;
		[Range(0, 1)] public float grabbingEasingFactor = .3f;

		[Header("Sound")]
		public AudioClip onFocusSound;
		public AudioClip onGrabSound;
		public AudioClip onDropSound;
		public List<AudioClip> stepAudioClips = new List<AudioClip>();
	}
}