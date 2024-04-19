using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

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

		public SpawnPoint FindSpawnPointByName(string name) {
			var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach(var spawnPoint in spawnPoints) {
				if(spawnPoint.Name != name)
					continue;
				return spawnPoint;
			}
			return null;
		}

		public void AlignSpawnPoints(string names) {
			var nameArr = names.Split(';');
			if(nameArr.Length <= 1)
				return;
			var anchor = FindSpawnPointByName(nameArr[0]);
			if(anchor == null) {
				Debug.LogWarning($"Warning: Cannot align spawn points to \"{nameArr[0]}\", as the target can't be found.");
				return;
			}
			for(int i = 1; i < nameArr.Length; ++i) {
				var name = nameArr[i];
				var alignee = FindSpawnPointByName(name);
				if(alignee == null) {
					Debug.LogWarning($"Warning: Cannot align spawn point \"{name}\" to {anchor}, as it can't be found.");
					continue;
				}
				AlignSpawnPoints(anchor, alignee);
			}
		}

		public void AlignSpawnPoints(string anchor, string alignee) {
			AlignSpawnPoints(FindSpawnPointByName(anchor), FindSpawnPointByName(alignee));
		}

		public void AlignSpawnPoints(SpawnPoint anchor, SpawnPoint alignee) {
			Debug.Log($"Aligning spawn point {anchor} to {alignee}.");
			if(anchor == null || alignee == null)
				return;

			var level = alignee.Level;
			Vector3 deltaPosition = anchor.transform.position - alignee.transform.position;
			level.transform.Translate(deltaPosition);
			Quaternion deltaOrientation = anchor.transform.rotation * Quaternion.Inverse(alignee.transform.rotation);
			level.transform.RotateAlong(alignee.transform.position, deltaOrientation);
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
			// Temporarily disables protagonist input when loading the level, or else the stuck
			// would cause bad experience.
			bool movement = UsesProtagonistMovement, orientation = UsesProtagonistOrientation;
			UsesProtagonistMovement = false;
			UsesProtagonistOrientation = false;

			var level = Instantiate(template.gameObject).GetComponent<Level>();
			TakeCareOfLevel(level);

			UsesProtagonistMovement = movement;
			UsesProtagonistOrientation = orientation;

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