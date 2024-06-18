using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	[System.Serializable]
	public struct LevelScene {
		public string name;
		[NaughtyAttributes.Scene] public int sceneIndex;
	}

	public struct LoadedLevel {
		public Level level;
		public LevelScene levelScene;
	}

	public partial class GameManager {
		#region Fields
		private readonly List<LoadedLevel> loadedLevels = new();
		private readonly List<Logic> levelLoadCallbacks = new();
		#endregion

		#region Interfaces
		public void LoadLevelByName(string name) {
			LoadedLevel? ll = FindLoadedLevelOfName(name);
			if(ll != null) {
				ll.Value.level.gameObject.SetActive(true);
				return;
			}
			LevelScene? levelScene = FindLevelSceneByName(name);
			if(levelScene == null) {
				Debug.LogWarning($"Warning: Cannot find level template of name \"{name}\".");
				return;
			}
			LoadLevel(levelScene.Value);
		}

		public void UnloadLevelByName(string name) {
			var ll = FindLoadedLevelOfName(name);
			if(ll == null) {
				Debug.LogWarning($"Warning: Cannot unload level \"{name}\" as it doesn't exist.");
				Debug.Log("Currently loaded levels: " + string.Join(", ", loadedLevels.Select(ll => ll.level.Name)));
				return;
			}
			UnloadLevel(ll.Value);
		}

		public void AddLevelLoadCallback(Logic logic) {
			if(logic == null) {
				Debug.LogWarning("Warning: The level load callback logic to be added is null. Skipping.");
				return;
			}
			levelLoadCallbacks.Add(logic);
		}

		public void DropLevelLoadCallbacks() {
			levelLoadCallbacks.Clear();
		}

		public SpawnPoint FindSpawnPointByName(string name) {
			return HierarchyUtility.FindObjectByName<SpawnPoint>(name);
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
			Debug.Log($"Aligning spawn point {alignee} to {anchor}.", anchor);
			if(anchor == null || alignee == null)
				return;

			var level = alignee.transform.GetLevel();
			if(level == null) {
				Debug.LogWarning($"Warning: Cannot get the containing level of {alignee}, aborting aligning spawn points.", alignee);
				return;
			}
			Vector3 deltaPosition = anchor.transform.position - alignee.transform.position;
			level.transform.position += deltaPosition;
			Quaternion deltaOrientation = anchor.transform.rotation * Quaternion.Inverse(alignee.transform.rotation);
			level.transform.RotateAlong(alignee.transform.position, deltaOrientation);
		}

		public void UnloadAllLevels() {
			DropLevelLoadCallbacks();
			foreach(var level in loadedLevels) {
				UnloadLevel(level);
			}
		}
		#endregion

		#region Life cycle
		protected void InitializeLevel() {
			var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
			// Take care of all already existing levels in the scene.
			foreach(var level in FindObjectsOfType<Level>(true)) {
				TakeCareOfLevel(new LoadedLevel {
					level = level,
					levelScene = new LevelScene {
						name = level.name,
						sceneIndex = currentSceneIndex,
					}
				});
			}
		}
		#endregion

		#region Functions
		private void TakeCareOfLevel(LoadedLevel ll) {
			if(ll.level == null)
				return;

			if(!ll.level.IsLoaded)
				ll.level.OnLoaded += () => OnLevelLoaded(ll);
			else
				OnLevelLoaded(ll);

			ll.level.OnUnloaded += () => OnLevelUnloaded(ll);
		}

		private void OnLevelLoaded(LoadedLevel ll) {
			loadedLevels.Add(ll);
			Debug.Log($"Level {ll.levelScene.name} loaded.", ll.level);
			foreach(var cb in levelLoadCallbacks) {
				if(cb == null || !cb.isActiveAndEnabled)
					continue;
				cb.Invoke();
			}
			DropLevelLoadCallbacks();
		}

		private void OnLevelUnloaded(LoadedLevel ll) {
			loadedLevels.Remove(ll);
		}

		private LevelScene? FindLevelSceneByName(string name) {
			foreach(var ls in Settings.levelScenes) {
				if(ls.name == name)
					return ls;
			}
			return null;
		}

		private void LoadLevel(LevelScene ls) {
			StartCoroutine(LoadLevelCoroutine(ls));
		}

		private System.Collections.IEnumerator LoadLevelCoroutine(LevelScene ls) {
			Debug.Log($"Loading level {ls.name}.");

			var loadOperation = SceneManager.LoadSceneAsync(ls.sceneIndex, LoadSceneMode.Additive);
			yield return new WaitUntil(() => loadOperation.isDone);

			Scene scene = SceneManager.GetSceneByBuildIndex(ls.sceneIndex);
			if(!scene.IsValid()) {
				Debug.LogWarning($"Failed to load level {ls.name}.");
				yield break;
			}

			yield return new WaitUntil(() => scene.isLoaded);

			var rootObjs = scene.GetRootGameObjects();
			Level level = null;
			foreach(var rootObj in rootObjs) {
				if(!rootObj.TryGetComponent(out level))
					continue;
				break;
			}
			if(level == null) {
				Debug.LogWarning($"Failed to load level {ls.name}.");
				SceneManager.UnloadSceneAsync(scene);
				yield break;
			}

			TakeCareOfLevel(new() {
				level = level,
				levelScene = ls,
			});
		}

		private LoadedLevel? FindLoadedLevelOfName(string name) {
			foreach(var ll in loadedLevels) {
				if(ll.level == null)
					continue;
				if(ll.levelScene.name != name)
					continue;
				return ll;
			}
			return null;
		}

		private void UnloadLevel(LoadedLevel ll) {
			StartCoroutine(UnloadLevelCoroutine(ll));
		}

		private System.Collections.IEnumerator UnloadLevelCoroutine(LoadedLevel ll) {
			Debug.Log($"Unloading level {ll.levelScene.name}.");
			ll.level.gameObject.SetActive(false);
			if(ll.levelScene.sceneIndex >= 0) {
				var operation = SceneManager.UnloadSceneAsync(ll.levelScene.sceneIndex);
				yield return new WaitUntil(() => operation.isDone);
			}
			else {
				Destroy(ll.level.gameObject);
			}
			Debug.Log($"Level {ll.levelScene.name} unloaded.");
		}
		#endregion
	}
}