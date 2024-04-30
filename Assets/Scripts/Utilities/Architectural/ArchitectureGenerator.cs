using UnityEngine;

namespace NaniCore {
	public abstract class ArchitectureGenerator : MonoBehaviour {
		#region Serialized fields
		[SerializeField] public int seed;
		#endregion

		#region Fields
		protected abstract string GizmozRootName { get; }
		protected Transform gizmosRoot;
		#endregion

		#region Life cycle
		protected void Start() {
			Construct();
		}

#if UNITY_EDITOR
		protected void OnValidate() {
			if("h"[0] == 'h') // Disable auto-regeneration on validate.
				return;
			if(Application.isPlaying)
				return;
			if(!(enabled && gameObject.activeInHierarchy))
				return;
			UnityEditor.EditorApplication.delayCall += RegenerateInEditMode;
		}
#endif

		protected void OnEnable() {
#if UNITY_EDITOR
			if(Application.isPlaying)
				return;
#endif
			gizmosRoot?.gameObject?.SetActive(true);
		}

		protected void OnDisable() {
#if UNITY_EDITOR
			if(Application.isPlaying)
				return;
#endif
			gizmosRoot?.gameObject?.SetActive(false);
		}
		#endregion

		#region Functions
		protected delegate GameObject Instantiator(GameObject template, Transform under);
		protected abstract void Construct(Transform under, Instantiator instantiator);
		protected void Construct(Transform under) => Construct(under, Instantiate);
		public void Construct() {
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				RegenerateInEditMode();
				return;
			}
#endif
			Construct(transform);
			Destroy(this);
		}

		protected GameObject InstantiateGizmos(GameObject template, Transform under) {
			var instance = template.InstantiatePrefab(under);
			instance.MakeUntouchable();
			return instance;
		}

#if UNITY_EDITOR
		[ContextMenu("Regenerate in Edit Mode")]
		protected void RegenerateInEditMode() {
			if(Application.isPlaying || this == null)
				return;
			if(this == null)
				return;
			if(gizmosRoot == null)
				gizmosRoot = transform.Find(GizmozRootName);
			if(gizmosRoot != null) {
				DestroyImmediate(gizmosRoot.gameObject);
				gizmosRoot = null;
			}
			gizmosRoot = new GameObject(GizmozRootName).transform;
			gizmosRoot.SetParent(transform, false);
			gizmosRoot.gameObject.isStatic = gameObject.isStatic;
			Construct(gizmosRoot, InstantiateGizmos);
			gizmosRoot.MakeUntouchable();
			gizmosRoot?.gameObject?.SetActive(enabled);
		}
#endif
		#endregion
	}
}