using UnityEngine;

namespace NaniCore.UnityPlayground {
	public abstract class Interaction : MonoBehaviour {
		public abstract void OnFocusEnter();
		public abstract void OnFocusLeave();
		public abstract void OnInteract();
	}
}