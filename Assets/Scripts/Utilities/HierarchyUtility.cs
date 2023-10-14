using UnityEngine;

namespace NaniCore {
	public static class HierarchyUtility {
		public static T EnsureComponent<T>(this GameObject gameObject) where T : Component {
			return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
		}

		public static T EnsureComponent<T>(this Component target) where T : Component
			=> target.gameObject.GetComponent<T>();
	}
}