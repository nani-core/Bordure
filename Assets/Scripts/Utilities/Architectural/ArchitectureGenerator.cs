using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaniCore {
	public abstract class ArchitectureGenerator : MonoBehaviour {
		#region Functions
		protected delegate GameObject Instantiator(GameObject template, Transform under);
		protected abstract void Construct(Transform under, Instantiator instantiator);
		protected void Construct(Transform under) => Construct(under, Instantiate);
		public void Construct() {
			if(!Application.isPlaying) {
#if UNITY_EDITOR
				RegenerateInEditMode();
#endif
			}
			else {
				Construct(transform);
				Destroy(this);
			}
		}
		#endregion

		#region Life cycle
		protected void Start() {
			Construct();
		}
		#endregion

#if UNITY_EDITOR
		#region Fields
		protected const HideFlags gizmosHideFlag = HideFlags.HideAndDontSave;
		protected abstract string GizmozRootName { get; }
		protected Transform gizmosRoot;
		#endregion

		#region Functions
		protected GameObject InstantiateGizmos(GameObject template, Transform under) {
			var instance = PrefabUtility.InstantiatePrefab(template, under) as GameObject;
			instance.hideFlags = gizmosHideFlag;
			return instance;
		}

		[ContextMenu("Regenerate in Edit Mode")]
		protected void RegenerateInEditMode() {
			if(Application.isPlaying)
				return;
			if (this == null)
				return;
			if(gizmosRoot == null)
				gizmosRoot = transform.Find(GizmozRootName);
			if(gizmosRoot != null) {
				DestroyImmediate(gizmosRoot.gameObject);
				gizmosRoot = null;
			}
			gizmosRoot = new GameObject(GizmozRootName).transform;
			gizmosRoot.SetParent(transform, false);
			gizmosRoot.gameObject.hideFlags = gizmosHideFlag;
			Construct(gizmosRoot, InstantiateGizmos);
			gizmosRoot?.gameObject?.SetActive(enabled);
		}
		#endregion

		#region Life cycle
		protected void OnValidate() {
			if(Application.isPlaying)
				return;
			if(!(enabled && gameObject.activeInHierarchy))
				return;
			EditorApplication.delayCall += RegenerateInEditMode;
		}

		protected void OnEnable() {
			if(Application.isPlaying)
				return;
			gizmosRoot?.gameObject?.SetActive(true);
		}

		protected void OnDisable() {
			if(Application.isPlaying)
				return;
			gizmosRoot?.gameObject?.SetActive(false);
		}
		#endregion
#endif
	}
}