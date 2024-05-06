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

		private static Matrix4x4 aligningAnchor;
		[MenuItem("Tools/Record Aligning Anchor")]
		public static void RecordAligningTarget() {
			var targetObj = Selection.activeGameObject;
			if(targetObj == null) {
				Debug.LogWarning("Please select a gameobject to be the aligning anchor.");
				return;
			}
			aligningAnchor = targetObj.transform.localToWorldMatrix;
			Debug.Log($"Aligning anchor is set to {targetObj.transform}.", targetObj);
		}

		[MenuItem("Tools/Align Level to Anchor by Target")]
		public static void AlignLevelToTarget() {
			var targetObj = Selection.activeGameObject;
			if(targetObj == null) {
				Debug.LogWarning("Please select a gameobject to be the aligning target.");
				return;
			}
			Level level = targetObj.GetComponentInParent<Level>();
			if(level == null) {
				Debug.LogWarning($"The selected aligning target ({targetObj}) does not belong to a level.", targetObj);
				return;
			}
			var anchor = new GameObject();
			anchor.name = "Aligning Anchor";
			anchor.transform.SetPositionAndRotation(aligningAnchor.GetPosition(), aligningAnchor.rotation);
			level.transform.AlignWith(targetObj.transform, anchor.transform);
			Debug.Log($"Aligned {level} to {anchor.transform.position} based on {targetObj}.");
			HierarchyUtility.Destroy(anchor);
		}
	}
}