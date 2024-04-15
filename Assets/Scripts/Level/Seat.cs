using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	public class Seat : MonoBehaviour {
		#region Serialized fields
		public bool canOrient = true;
		public bool canLeaveManually = true;

		public UnityEvent onSitOn, onLeft;
		#endregion

		#region Message handlers
		protected void OnSitOn() {
			onSitOn?.Invoke();
		}

		protected void OnLeft() {
			onLeft?.Invoke();
		}
		#endregion
	}
}