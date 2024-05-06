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
		#endregion

		#region Interfaces
		public LoadedLevel? LoadLevelByName(string name) {
			LoadedLevel? ll = FindLoadedLevelOfName(name);
			if(ll != null) {
				ll.Value.level.gameObject.SetActive(true);
				return ll.Value;
			}
			LevelScene? levelScene = FindLevelSceneByName(name);
			if(levelScene == null) {
				Debug.LogWarning($"Warning: Cannot find level template of name \"{name}\".");
				return null;
			}
			return LoadLevel(levelScene.Value);
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

			var level = alignee.Level;
			if(level == null) {
				Debug.LogWarning($"Warning: Cannot get the containing level of {alignee}, aborting aligning spawn points.", alignee);
				return;
			}
			Vector3 deltaPosition = anchor.transform.position - alignee.transform.position;
			level.transform.position += deltaPosition;
			Quaternion deltaOrientation = anchor.transform.rotation * Quaternion.Inverse(alignee.transform.rotation);
			level.transform.RotateAlong(alignee.transform.position, deltaOrientation);
		}
		#endregion

		#region Life cycle
		protected void InitializeLevel() {
			// Take care of all already existing levels in the scene.
			foreach(var level in FindObjectsOfType<Level>(true))
				TakeCareOfLevel(new LoadedLevel {
					level = level,
					levelScene = new LevelScene {
						name = level.name,
						sceneIndex = -1,
					}
				});
		}
		#endregion

		#region Functions
		private void TakeCareOfLevel(LoadedLevel ll) {
			ll.level.OnLoaded += () => OnLevelLoaded(ll);
			ll.level.OnUnloaded += () => OnLevelUnloaded(ll);
		}

		private void OnLevelLoaded(LoadedLevel ll) {
			loadedLevels.Add(ll);
			Debug.Log($"Level {ll.levelScene.name} loaded.", ll.level);
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

		private LoadedLevel? LoadLevel(LevelScene ls) {
			Debug.Log($"Loading level {ls.name}.");
			var scene = SceneManager.LoadScene(ls.sceneIndex, new LoadSceneParameters {
				loadSceneMode = LoadSceneMode.Additive,
				localPhysicsMode = LocalPhysicsMode.Physics3D,
			});
			var level = scene.GetRootGameObjects().OfType<Level>().FirstOrDefault();
			if(level == null) {
				Debug.LogWarning($"Failed to load level {ls.name}.");
				return null;
			}
			return new LoadedLevel {
				level = level,
				levelScene = ls,
			};
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
			ll.level.gameObject.SetActive(false);
			if(ll.levelScene.sceneIndex >= 0) {
				SceneManager.UnloadSceneAsync(ll.levelScene.sceneIndex);
			}
			else {
				Destroy(ll.level.gameObject);
			}
			Debug.Log($"Level {ll.levelScene.name} unloaded.");
		}
		#endregion
	}
}