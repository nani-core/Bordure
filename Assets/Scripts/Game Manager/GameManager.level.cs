using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Stencil {
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

		public void DestroyLevel(string levelName) {
			if(!loadedLevels.ContainsKey(levelName))
				return;
			Destroy(loadedLevels[levelName]);
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
			level.onLoaded += OnLevelLoaded;
			level.onUnloaded += OnLevelUnloaded;
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
		#endregion
	}
}