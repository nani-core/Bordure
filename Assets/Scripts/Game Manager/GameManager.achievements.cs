using UnityEngine;
using System.Linq;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[SerializeField] private AchievementManager achievement;
		#endregion

		#region Fields
		private int duckCount = 0;
		#endregion

		#region Interfaces
		public AchievementManager Achievement => achievement;

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

		public void FinishSpeedrunAchievement(float runTime) {
			var levels = achievement.Sheet.speedrunLevels.ToList();
			levels.Sort((a, b) => (int)Mathf.Sign(a.maxTime - b.maxTime));
			foreach(var level in levels) {
				if(runTime > level.maxTime)
					continue;
				FinishAchievement(level.key);
				return;
			}
			FinishAchievement(achievement.Sheet.levelFinishKey);
		}
		#endregion
	}
}