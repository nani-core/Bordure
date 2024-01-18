using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	[CreateAssetMenu(menuName = "Nani Core/Game Settings")]
	public class GameSettings : ScriptableObject {
		[Header("Protagonist")]
		public Protagonist protagonist;
		public ProtagonistProfile protagonistProfile;

		[Header("Rigidbody")]
		public AudioClip collisionSound;	// Only temporary.
	}
}