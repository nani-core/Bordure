using UnityEngine;

namespace NaniCore.Loopool {
	public class Detachable : Interactable {
		#region Message handlers
		protected override void OnInteract() {
			Detach();
		}
		#endregion

		#region Functions
		public void Detach() {
			transform.SetParent(null, true);
			if(Rigidbody) {
				Rigidbody.isKinematic = false;
			}

			SendMessage("OnDetach", SendMessageOptions.DontRequireReceiver);
			enabled = false;
		}
		#endregion

		#region Life cycle
		protected void Start() {
			if(Rigidbody) {
				Rigidbody.isKinematic = true;
			}
		}
		#endregion
	}
}