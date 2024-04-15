using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	public class Level : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private UnityEvent onLoaded;
		#endregion

		#region Interfaces
		public System.Action OnLoaded, OnUnloaded;
		#endregion

		#region Life cycle
		protected void Start() {
			OnLoaded += onLoaded.Invoke;
			OnLoaded?.Invoke();
		}

		protected void OnDestroy() {
			OnUnloaded?.Invoke();
		}
		#endregion
	}
}
