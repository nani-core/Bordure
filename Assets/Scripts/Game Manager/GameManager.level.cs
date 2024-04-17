using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private readonly Dictionary<string, Level> loadedLevels = new();
		#endregion

		#region Interfaces
		public IEnumerable<Level> LoadedLevels => loadedLevels.Values;

		public Level LoadLevelByName(string name) {
			Level level = FindLoadedLevelOfName(name);
			if(level == null) {
				Level template = FindLevelTemplateByName(name);
				if(template == null) {
					Debug.LogWarning($"Warning: Cannot find level template of name \"{name}\".");
					return null;
				}
				level = InstantiateLevelTemplate(name, template);
			}
			level.gameObject.SetActive(true);
			return level;
		}

		public void UnloadLevelByName(string levelName) {
			if(!loadedLevels.ContainsKey(levelName)) {
				Debug.LogWarning($"Warning: Cannot unload level \"{levelName}\" as it doesn't exist.");
				Debug.Log("Current loaded levels: " + string.Join(", ", loadedLevels.Keys));
				return;
			}
			Level level = loadedLevels[levelName];
			UnloadLevel(level);
		}

		#endregion

		#region Life cycle
		protected void InitializeLevel() {
			// Take care of all already existing levels in the scene.
			foreach(var level in FindObjectsOfType<Level>(true))
				TakeCareOfLevel(level);
		}
		#endregion

		#region Functions
		private void TakeCareOfLevel(Level level) {
			level.OnLoaded += () => OnLevelLoaded(level);
			level.OnUnloaded += () => OnLevelUnloaded(level);
		}

		private void OnLevelLoaded(Level level) {
			loadedLevels.Add(level.name, level);
		}

		private void OnLevelUnloaded(Level level) {
			loadedLevels.Remove(level.name);
		}

		private Level InstantiateLevelTemplate(string name, Level template) {
			var level = Instantiate(template.gameObject).GetComponent<Level>();
			level.name = name;
			TakeCareOfLevel(level);

			return level;
		}

		private Level FindLevelTemplateByName(string name) {
			foreach(var entry in Settings.levelTemplates) {
				if(entry.name == name)
					return entry.level;
			}
			return null;
		}

		private Level FindLoadedLevelOfName(string levelName) {
			if(!loadedLevels.ContainsKey(levelName))
				return null;
			return loadedLevels[levelName];
		}

		private void UnloadLevel(Level level) {
			level.gameObject.SetActive(false);
			//HierarchyUtility.Destroy(level);
		}
		#endregion
	}
}