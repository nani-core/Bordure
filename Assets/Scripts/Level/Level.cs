using UnityEngine;

namespace NaniCore.Bordure {
	public class Level : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private new string name;
		#endregion

		#region Interfaces
		public string Name => name;
		public System.Action OnLoaded, OnUnloaded;
		#endregion

		#region Life cycle
		protected void Start() {
			OnLoaded?.Invoke();
		}

		protected void OnDestroy() {
			OnUnloaded?.Invoke();
		}
		#endregion
	}
}
