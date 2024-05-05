using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {

	public static class EditorUtility {
		[MenuItem("Tools/Remove Hidden Scene ID Map Objects")]
		public static void RemoveHiddenSceneIdMap() {
			int count = 0;
			for(GameObject target; (target = GameObject.Find("SceneIDMap")) != null; ++count) {
				Object.DestroyImmediate(target);
			}
			Debug.Log($"Removed {count} hidden scene ID map objects.");
		}

		[MenuItem("Tools/Ungarrison All Architectures")]
		public static void UngarrisonAllArchitectures() {
			List<GameObject> dirtyObjects = new();

			var boxes = Object.FindObjectsByType<ConcreteBox>(FindObjectsSortMode.None).ToArray();
			dirtyObjects.AddRange(boxes.Select(box => box.gameObject));
			var tiles = Object.FindObjectsByType<MeshTile>(FindObjectsSortMode.None).ToArray();
			dirtyObjects.AddRange(tiles.Select(tile => tile.gameObject));

			foreach(var box in boxes) {
				var boxObj = box.gameObject;
				box.Ungarrison();
				foreach(var subtile in boxObj.GetComponentsInChildren<MeshTile>().ToArray()) {
					subtile.Ungarrison();
				}
			}

			foreach(var tile in tiles) {
				var tileObj = tile.gameObject;
				tile.Ungarrison();
			}

			Debug.Log($"Ungarrisoned {dirtyObjects.Count} architectures.");
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
	}
}