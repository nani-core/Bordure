using UnityEngine;

namespace NaniCore.Stencil {
	[RequireComponent(typeof(Loopshape))]
	public abstract class LoopshapeValidator : MonoBehaviour {
		#region Fields
		private Loopshape loopshape;
		private bool isValid = false;
		#endregion

		#region Interfaces
		public Loopshape LoopShape => loopshape;
		public bool IsValid => isValid;

		protected abstract bool Validate();
		#endregion

		#region Life cycle
		protected void Start() {
			loopshape = GetComponent<Loopshape>();
		}

		protected void Update() {
			bool isNowValid = Validate();
			if(isValid != isNowValid) {
				string message = isNowValid ? "OnLoopShapeValidated" : "OnLoopShapeInvalidated";
				SendMessage(message, SendMessageOptions.DontRequireReceiver);
			}
			isValid = isNowValid;
		}
		#endregion
	}
}