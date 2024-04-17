using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private readonly List<Level> loadedLevels = new();
		#endregion

		#region Interfaces
		public IEnumerable<Level> LoadedLevels => loadedLevels;

		public Level LoadLevelByName(string name) {
			Level level = FindLoadedLevelOfName(name);
			if(level == null) {
				Level template = FindLevelTemplateByName(name);
				if(template == null) {
					Debug.LogWarning($"Warning: Cannot find level template of name \"{name}\".");
					return null;
				}
				level = InstantiateLevelTemplate(template);
			}
			level.gameObject.SetActive(true);
			return level;
		}

		public void HideLevelByName(string name) {
			var level = FindLoadedLevelOfName(name);
			if(level == null) {
				Debug.LogWarning($"Warning: Cannot hide level \"{name}\" as it doesn't exist.");
				Debug.Log("Current loaded levels: " + string.Join(", ", loadedLevels.Select(level => level.Name)));
				return;
			}
			HideLevel(level);
		}

		public void UnloadLevelByName(string name) {
			var level = FindLoadedLevelOfName(name);
			if(level == null) {
				Debug.LogWarning($"Warning: Cannot unload level \"{name}\" as it doesn't exist.");
				Debug.Log("Current loaded levels: " + string.Join(", ", loadedLevels.Select(level => level.Name)));
				return;
			}
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
			loadedLevels.Add(level);
		}

		private void OnLevelUnloaded(Level level) {
			loadedLevels.Remove(level);
		}

		private Level InstantiateLevelTemplate(Level template) {
			var level = Instantiate(template.gameObject).GetComponent<Level>();
			TakeCareOfLevel(level);

			return level;
		}

		private Level FindLevelTemplateByName(string name) {
			foreach(var level in Settings.levelTemplates) {
				if(level == null)
					continue;
				if(level.Name == name)
					return level;
			}
			return null;
		}

		private Level FindLoadedLevelOfName(string name) {
			foreach(var level in loadedLevels) {
				if(level == null)
					continue;
				if(level.Name != name)
					continue;
				return level;
			}
			return null;
		}

		private void HideLevel(Level level) {
			level.gameObject.SetActive(false);
		}

		private void UnloadLevel(Level level) {
			StartCoroutine(UnloadLevelInNextFrameCoroutine(level));
		}

		private System.Collections.IEnumerator UnloadLevelInNextFrameCoroutine(Level level) {
			HideLevel(level);
			yield return new WaitForEndOfFrame();
			HierarchyUtility.Destroy(level);
		}
		#endregion
	}
}