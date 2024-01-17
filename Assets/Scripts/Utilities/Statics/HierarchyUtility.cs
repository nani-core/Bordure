using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace NaniCore {
	public static class HierarchyUtility {
		#region Component
		public static T EnsureComponent<T>(this GameObject gameObject) where T : Component {
			if(gameObject.TryGetComponent<T>(out var existing))
				return existing;
			return gameObject.AddComponent<T>();
		}

		public static T EnsureComponent<T>(this Component target) where T : Component
			=> target.gameObject.GetComponent<T>();
		#endregion

		#region Prefab
		/// <summary>
		/// Create an instance of a prefab, editor friendly.
		/// </summary>
		public static GameObject InstantiatePrefab(this GameObject template, Transform under = null) {
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				if(PrefabUtility.IsPartOfPrefabAsset(template))
					return PrefabUtility.InstantiatePrefab(template, under) as GameObject;
			}
#endif
			return Object.Instantiate(template, under);
		}

		/// <remarks>Will not throw exceptions when the target is not a prefab instance.</remarks>
		public static void RestorePrefabInstance(this GameObject target) {
#if UNITY_EDITOR
			if(Application.isPlaying)
				return;
			if(!PrefabUtility.IsPartOfPrefabInstance(target))
				return;
			PrefabUtility.RevertPrefabInstance(target, InteractionMode.AutomatedAction);
#endif
		}
		#endregion

		#region Serilization
		/// <summary>
		/// Make something not selectable nor savable nor visible in hierarchy.
		/// </summary>
		public static void MakeUntouchable(this Object target, bool hideInEditor = true, bool hideInScene = false) {
			GameObject realTarget;
			switch(target) {
				case GameObject go:
					realTarget = go;
					break;
				case Component component:
					realTarget = component.gameObject;
					break;
				default:
					return;
			}
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				var svm = SceneVisibilityManager.instance;
				if(hideInScene)
					svm.Hide(realTarget, true);
				else
					svm.Show(realTarget, false);
				svm.DisablePicking(realTarget, true);
			}
#endif
			var hideFlags = realTarget.hideFlags;
			hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			if(hideInEditor)
				hideFlags |= HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			realTarget.hideFlags = hideFlags;
		}

		public static IEnumerable<T> FindAllComponentsInEditor<T>(this GameObject root, bool includeInactive = false) where T : Component {
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				foreach(Transform child in root.transform) {
					if(child.TryGetComponent<T>(out var here))
						yield return here;
					foreach(var grandchild in child.gameObject.FindAllComponentsInEditor<T>(includeInactive))
						yield return grandchild;
				}
				yield break;
			}
#endif
			foreach(var component in root.GetComponentsInChildren<T>(includeInactive))
				yield return component;
		}
		#endregion
	}
}