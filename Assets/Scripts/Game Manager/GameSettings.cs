using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	[CreateAssetMenu(menuName = "Nani Core/Game Settings")]
	public class GameSettings : ScriptableObject {
		public Protagonist protagonist;
		public ProtagonistProfile protagonistProfile;
	}
}