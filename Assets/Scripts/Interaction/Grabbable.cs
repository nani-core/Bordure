using UnityEngine;

namespace NaniCore.UnityPlayground {
	public class Grabbable : Interaction {
		protected override void OnFocusEnter() {
		}

		protected override void OnFocusLeave() {
		}

		protected override void OnInteract() {
			Protagonist.instance.Grabbing = this;
		}

		protected void OnGrabStart() {
		}

		protected void OnGrabEnd() {
		}
	}
}