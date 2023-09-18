#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NaniCore {
	public partial class MeshTile : MonoBehaviour {
		#region Fields
		private const string gizmosRootName = "$MeshTileGizmosRoot";
		private const HideFlags gizmosHideFlag = HideFlags.HideAndDontSave;
		private Transform gizmosRoot;
		#endregion

		#region Functions
		private GameObject InstantiateGizmos(GameObject template, Transform under) {
			var instance = PrefabUtility.InstantiatePrefab(template, under) as GameObject;
			instance.hideFlags = gizmosHideFlag;
			return instance;
		}

		[ContextMenu("Regenerate in Edit Mode")]
		private void RegenerateInEditMode() {
			if(Application.isPlaying)
				return;
			if(gizmosRoot == null)
				gizmosRoot = transform.Find(gizmosRootName);
			if(gizmosRoot != null) {
				DestroyImmediate(gizmosRoot.gameObject);
				gizmosRoot = null;
			}
			gizmosRoot = new GameObject(gizmosRootName).transform;
			gizmosRoot.SetParent(transform, false);
			gizmosRoot.gameObject.hideFlags = gizmosHideFlag;
			Construct(gizmosRoot, InstantiateGizmos);
			gizmosRoot?.gameObject?.SetActive(enabled);
		}
		#endregion

		#region Life cycle
		private void OnValidate() {
			if(Application.isPlaying)
				return;
			EditorApplication.delayCall += RegenerateInEditMode;
		}

		private void OnEnable() {
			if(Application.isPlaying)
				return;
			gizmosRoot?.gameObject?.SetActive(true);
		}

		private void OnDisable() {
			if(Application.isPlaying)
				return;
			gizmosRoot?.gameObject?.SetActive(false);
		}
		#endregion
	}
}
#endif