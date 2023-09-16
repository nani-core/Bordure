using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Loopool {
	public class Clickable : Interactable {
		#region Serialized fields
		[SerializeField] protected UnityEvent onInteract;
		#endregion

		#region Message handlers
		protected override void OnInteract() {
			onInteract?.Invoke();
		}
		#endregion
	}
}