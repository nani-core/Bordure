using UnityEngine;

namespace NaniCore.Bordure {
	[System.Serializable]
	public struct AchievementEntry {
		public string key;
		public Sprite icon;
		public string title;
		public string description;
	}

	[System.Serializable]
	public struct SpeedrunLevel {
		[Tooltip("In seconds")][Min(0)] public float maxTime;
		public string key;
	}

	[CreateAssetMenu(menuName = "Nani Core/Achievement Sheet")]
	public class AchievementSheet : ScriptableObject {
		public AchievementEntry[] achievements;
		public string levelFinishKey;
		public SpeedrunLevel[] speedrunLevels;
	}
}
