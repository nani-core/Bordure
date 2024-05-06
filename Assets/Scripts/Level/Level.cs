using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public class Level : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private new string name;
		[SerializeField] private List<LevelSection> preloadedSections = new();
		#endregion

		#region Fields
		private bool isLoaded = false;
		#endregion

		#region Interfaces
		public string Name => name;
		public System.Action OnLoaded, OnUnloaded;
		public bool IsLoaded => isLoaded;
		#endregion

		#region Life cycle
		protected void Start() {
			foreach(var section in preloadedSections) {
				section.Load();
			}

			OnLoaded?.Invoke();
			isLoaded = true;
		}

		protected void OnDestroy() {
			OnUnloaded?.Invoke();
		}
		#endregion
	}

	public static class LevelUtility {
		public static Level GetLevel(this Transform target) {
			return target.GetComponentInParent<Level>(true);
		}
	}
}
