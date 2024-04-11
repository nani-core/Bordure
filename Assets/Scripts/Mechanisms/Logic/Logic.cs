using UnityEngine;
using UnityEngine.Events;

namespace NaniCore {
	public class Logic : MonoBehaviour {
		#region Serialized fields
		public UnityEvent callback;
		#endregion

		#region Interfaces
		public virtual void Invoke() {
			callback?.Invoke();
		}
		#endregion
	}
}