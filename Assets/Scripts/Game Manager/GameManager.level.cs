using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private readonly Dictionary<string, Level> loadedLevels = new();
		#endregion

		#region Interfaces
		public IEnumerable<Level> LoadedLevels => loadedLevels.Values;

		public Level FindLoadedLevelOfName(string levelName) {
			if(!loadedLevels.ContainsKey(levelName))
				return null;
			return loadedLevels[levelName];
		}

		public Level LoadLevel(Level template) {
			Level level = FindLoadedLevelOfName(template.name);
			if(level == null)
				level = InstantiateLevel(template);

			level.gameObject.SetActive(true);
			return level;
		}

		public void UnloadLevelByName(string levelName) {
			if(!loadedLevels.ContainsKey(levelName)) {
				Debug.LogWarning($"Warning: Cannot unload level \"${levelName}\" as it doesn't exist.");
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

		private Level InstantiateLevel(Level template) {
			var level = Instantiate(template.gameObject).GetComponent<Level>();
			level.name = template.name;
			TakeCareOfLevel(level);

			return level;
		}

		private void UnloadLevel(Level level) {
			level.gameObject.SetActive(false);
			//HierarchyUtility.Destroy(level);
		}
		#endregion
	}
}