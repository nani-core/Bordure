using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[SerializeField] private AchievementManager achievement;
		#endregion

		#region Fields
		private readonly HashSet<string> triggeredDucks = new();
		private readonly HashSet<Light> allLights = new();
		#endregion

		#region Interfaces
		public AchievementManager Achievement => achievement;

		public void FinishAchievement(string key) {
			achievement.Finish(key);
		}

		public void ResetAchievementProgress() {
			achievement.ResetProgress();
			triggeredDucks.Clear();
			allLights.Clear();
		}

		public void TriggerDuckAchievement(string key) {
			if(triggeredDucks.Contains(key))
				return;

			triggeredDucks.Add(key);
			achievement.Finish($"duck{triggeredDucks.Count}");
		}

		public void FinishSpeedrunAchievement(float runTime) {
			FinishAchievement(achievement.Sheet.levelFinishKey);

			var levels = achievement.Sheet.speedrunLevels.ToList();
			levels.Sort((a, b) => (int)Mathf.Sign(a.maxTime - b.maxTime));
			foreach(var level in levels) {
				if(runTime > level.maxTime)
					continue;
				FinishAchievement(level.key);
				break;
			}
		}

		public void TriggerLightOffAchievement(Light light) {
			achievement.Finish("light off");

			ClearInvalidatedLights();
			if(allLights.All(light => !light.isActiveAndEnabled)) {
				achievement.Finish("light all off");
			}
		}
		#endregion

		#region Functions
		private void RegisterLightsInLevel(Level level) {
			foreach(Light light in level.transform.GetComponentsInChildren<Light>(true)) {
				if(light.GetComponentsInParent<Loopshape>() == null)
					continue;

				allLights.Add(light);
			}
		}

		private void ClearInvalidatedLights() {
			allLights.RemoveWhere(light => light == null);
		}
		#endregion
	}
}