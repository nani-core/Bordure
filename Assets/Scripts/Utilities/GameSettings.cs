using UnityEngine;

namespace NaniCore.Bordure {
	[CreateAssetMenu(menuName = "Nani Core/Game Settings")]
	public class GameSettings : ScriptableObject {
		[Header("Protagonist")]
		public Protagonist protagonist;
		[NaughtyAttributes.Expandable] public ProtagonistProfile protagonistProfile;

		[Header("Audio")]
		[NaughtyAttributes.Expandable] public AudioSettings audio;
		public bool makeAudioLogs = false;

		[Header("Water")]
		public Material waterStreamMaterial;
		public Vector3 defaultWaterEjectionVelocity = Vector3.forward;

		[Header("Highlight")]
		public Color highlightColor = Color.white;
		[Min(0)] public float highlightDuration = 1.0f;
		[Min(0)] public float highlightEmmisiveIntensity = 1.0f;
		[Min(0)] public float lightEmmisiveIntensity = 1.0f;
		public Material highlightMaterial;
		public Material highlightOffMaterial;

		[Header("Optical Loopshape")]
		/// <summary>
		/// The standard thickness of the bordure, measured in portion.
		/// </summary>
		[Range(0, 1)] public float bordureThicknessRatio = 1f / 6;
		/// <summary>
		/// In the last step of the validation algorithm, how much remaining areas are allowed.
		/// </summary>
		/// <remarks>
		/// Measured in the portion of the bordure width.
		/// </remarks>
		[Range(0, 1)] public float bordureThicknessTolerance = 0.3f;

		[Header("Optical Loopshape Validation")]
		[Min(0)] public float maxOpticalValidationInterval = 1f;
		[Min(0)] public float desiredOpticalValidationInterval = .1f;
		/// <summary>
		/// The standard height of the RT on which the validation algorithm will be performed.
		/// </summary>
		/// <remarks>
		/// The higher this value is, the more accurate the validation algorithm's result will be;
		/// at the meantime the performance cost will be higher.
		/// </remarks>
		[Min(1)] public int standardHeight = 216;

		#if DEBUG && UNITY_EDITOR
		[Header("Architecture Generation")]
		public bool generateConcreteInEditMode = true;
#endif

		[Header("Level")]
		public float deathHeight = -200f;
		public LevelScene[] levelScenes;
	}
}