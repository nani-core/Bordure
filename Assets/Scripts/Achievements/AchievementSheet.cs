using UnityEngine;

namespace NaniCore.Bordure {
	[System.Serializable]
	public struct AchievementEntry {
		public string key;
		public Sprite icon;
		public string title;
		public string description;
	}

	[CreateAssetMenu(menuName = "Nani Core/Achievement Sheet")]
	public class AchievementSheet : ScriptableObject {
		public AchievementEntry[] achievements;
	}
}
