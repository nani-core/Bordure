using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[SerializeField] private AchievementManager achievement;
		#endregion

		#region Fields
		private int duckCount = 0;
		#endregion

		#region Interfaces
		public void FinishAchievement(string key) {
			achievement.Finish(key);
		}

		public void ResetAchievementProgress() {
			achievement.ResetProgress();
			duckCount = 0;
		}

		public void IncreaseDuckAchievementCount() {
			++duckCount;
			FinishAchievement($"duck{duckCount}");
		}
		#endregion
	}
}