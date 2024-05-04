using UnityEngine;
using UnityEditor;

namespace NaniCore.Bordure {
	public static class EditorUtility {
		[MenuItem("Tools/Remove Hidden Scene ID Map Objects")]
		public static void RemoveHiddenSceneIdMap() {
			for(GameObject target; (target = GameObject.Find("SceneIDMap")) != null;) {
				Object.DestroyImmediate(target);
			}
		}
	}
}