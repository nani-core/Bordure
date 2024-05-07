using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using System.Linq;

namespace NaniCore {
	public static class HierarchyUtility {
		#region Component
		public static T EnsureComponent<T>(this GameObject gameObject) where T : Component {
			if(gameObject.TryGetComponent<T>(out var existing))
				return existing;
			return gameObject.AddComponent<T>();
		}

		public static T EnsureComponent<T>(this Component target) where T : Component
			=> target.gameObject.EnsureComponent<T>();
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

		public static bool IsInPrefabMode() {
#if UNITY_EDITOR
			return PrefabStageUtility.GetCurrentPrefabStage() != null;
#else
			return false;
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
		#endregion

		#region Hierarchical
		public static bool IsChildOf(this GameObject a, GameObject b) {
			if(a == null)
				return false;
			if(b == null)
				return true;
			return a.transform.IsChildOf(b.transform);
		}
		public static bool IsChildOf(this Transform a, GameObject b) => a?.gameObject.IsChildOf(b) ?? false;
		public static bool IsChildOf(this GameObject a, Transform b) => a.IsChildOf(b?.gameObject);

		public static Transform[] Children(this Transform parent) {
			Transform[] children = new Transform[parent.childCount];
			for(int i = 0; i < parent.childCount; ++i)
				children[i] = parent.GetChild(i);
			return children;
		}

		public static void Destroy(this Object target) {
#if UNITY_EDITOR
			if(Application.isPlaying)
				Object.Destroy(target);
			else
				Object.DestroyImmediate(target);
#else
			Object.Destroy(target);
#endif
		}

		public static void DestroyAllChildren(this Transform parent) {
			foreach(var child in parent.Children())
				Destroy(child.gameObject);
		}

		public static IEnumerable<T> FindObjectsByName<T>(string name, bool includeInactive = false) where T : Component {
			FindObjectsInactive includingFlag = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
			var instances = Object.FindObjectsByType<T>(includingFlag, FindObjectsSortMode.None);
			return instances.Where(i => i.name == name);
		}

		public static T FindObjectByName<T>(string name, bool includeInactive = false) where T : Component {
			return FindObjectsByName<T>(name, includeInactive).FirstOrDefault();
		}
		#endregion

		#region Shape & transformation
		public static void RotateAlong(this Transform target, Vector3 pivot, Quaternion rotation) {
			Transform pivotTransform = new GameObject().transform;
			pivotTransform.position = pivot;
			var parent = target.parent;
			target.SetParent(pivotTransform, true);
			pivotTransform.rotation = rotation;
			target.SetParent(parent, true);
			Destroy(pivotTransform.gameObject);
		}

		/// <param name="target">The GameObject to ge aligned in space.</param>
		/// <param name="reference">What to align by.</param>
		/// <param name="alignment">What to align to.</param>
		public static void AlignWith(this Transform target, Transform reference, Transform alignment) {
			Vector3 movement = alignment.position - reference.position;
			Quaternion rotation = alignment.rotation * Quaternion.Inverse(reference.rotation);

			target.RotateAlong(reference.position, rotation);
			target.position += movement;
		}
		public static void AlignWith(this Transform target, Transform alignment) {
			target.SetPositionAndRotation(alignment.position, alignment.rotation);
		}

		public static void Lerp(this Transform target, Transform start, Transform end, float t) {
			Vector3 position = Vector3.Lerp(start.position, end.position, t);
			Quaternion orientation = Quaternion.Slerp(start.rotation, end.rotation, t);

			if(target.TryGetComponent(out Rigidbody rb)) {
				rb.MovePosition(position);
				rb.MoveRotation(orientation);
			}
			else {
				target.SetPositionAndRotation(position, orientation);
			}
		}

		public static Bounds CalculateBoundingBox(this Transform parent) {
			Bounds res = new() {
				center = parent.position,
				size = Vector3.zero,
			};

			if(parent.TryGetComponent(out Renderer renderer))
				res = MathUtility.BoundingUnion(renderer.bounds, res);

			if(parent.TryGetComponent(out Collider collider))
				res = MathUtility.BoundingUnion(collider.bounds, res);

			foreach(var child in parent.Children())
				res = MathUtility.BoundingUnion(res, CalculateBoundingBox(child));

			return res;
		}
		#endregion
	}
}