using UnityEngine;

namespace NaniCore.Bordure {
	public class Level : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private new string name;
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
			OnLoaded?.Invoke();
			isLoaded = true;
		}

		protected void OnDestroy() {
			OnUnloaded?.Invoke();
		}
		#endregion
	}
}
