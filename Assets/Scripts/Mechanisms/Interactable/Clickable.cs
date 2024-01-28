using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Stencil {
	public class Clickable : Interactable {
		#region Serialized fields
		[SerializeField] protected UnityEvent onClicked;
		#endregion

		#region Message handlers
		protected override void OnInteract() {
			if(!isActiveAndEnabled)
				return;
			onClicked?.Invoke();
		}
		#endregion
	}
}