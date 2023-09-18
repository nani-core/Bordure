using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Loopool {
	public class Clickable : Interactable {
		#region Serialized fields
		[SerializeField] protected UnityEvent onClick;
		#endregion

		#region Message handlers
		protected override void OnInteract() {
			if(!isActiveAndEnabled)
				return;
			onClick?.Invoke();
		}
		#endregion
	}
}