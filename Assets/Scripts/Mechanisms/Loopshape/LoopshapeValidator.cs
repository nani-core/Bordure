using UnityEngine;

namespace NaniCore.Stencil {
	public abstract class LoopshapeValidator : MonoBehaviour {
		#region Fields
		private bool isValid = false;
		#endregion

		#region Interfaces
		public bool IsValid => isValid;
		#endregion

		#region Functions
		protected abstract bool Validate();
		#endregion

		#region Life cycle
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