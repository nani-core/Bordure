using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Loopool {
	public static class LevelUtility {
		public struct LevelRegistry {
			public Level template;
			public Level instance;
		}
		private static readonly List<LevelRegistry> levels = new();

		#region Interfaces
		public static Level InstantiateLevelFromTemplate(this Level template, Vector3 position, Quaternion orientation) {
			if(template == null) {
				Debug.LogWarning("The level to be instantiated is null.");
				return null;
			}

			var level = Object.Instantiate(template.gameObject).GetComponent<Level>();

			level.onLoaded += (Level level) => {
				levels.Add(new() {
					template = template,
					instance = level,
				});
			};
			level.onUnloaded += (Level level) => {
				levels.RemoveAll(registry => registry.instance == level);
			};

			level.transform.SetLocalPositionAndRotation(position, orientation);

			return level;
		}
		public static Level InstantiateLevelFromTemplate(this Level template, Transform place)
			=> InstantiateLevelFromTemplate(template, place.position, place.rotation);
		
		public static Level InstantiateLevelFromTemplateAtSpawnPoint(this Level template) {
			var level = InstantiateLevelFromTemplate(template, default, default);

			var sp = level.SpawnPoint;
			var protagonist = GameManager.Instance?.Protagonist;
			if(sp == null)
				Debug.LogWarning("The instantiated level does not have a spawn point.", level);
			else if(protagonist == null)
				Debug.LogWarning("There is no protagonist in the scene to be aligned with.", level);
			else {
				level.transform.Translate(protagonist.transform.position - sp.transform.position);
			}

			return level;
		}

		public static void DestroyEveryLevelInstanceOfTemplate(this Level template) {
			var targets = levels.FindAll(registry => registry.template == template);
			foreach(var target in targets)
				Object.Destroy(target.instance);
		}
		#endregion
	}
}