using UnityEngine;

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

		#region Fields
		protected abstract string GizmozRootName { get; }
		protected Transform gizmosRoot;
		#endregion

		#region Functions
		protected GameObject InstantiateGizmos(GameObject template, Transform under) {
			var instance = template.InstantiatePrefab(under);
			instance.MakeUntouchable();
			return instance;
		}

#if UNITY_EDITOR
		[ContextMenu("Regenerate in Edit Mode")]
		protected void RegenerateInEditMode() {
			if(Application.isPlaying)
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
			gizmosRoot.MakeUntouchable(false);
			Construct(gizmosRoot, InstantiateGizmos);
			gizmosRoot?.gameObject?.SetActive(enabled);
		}
#endif
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		protected void OnValidate() {
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
	}
}