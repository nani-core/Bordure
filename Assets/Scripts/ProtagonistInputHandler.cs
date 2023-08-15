using UnityEngine;
using UnityEngine.InputSystem;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(Protagonist))]
	[RequireComponent(typeof(PlayerInput))]
	public class ProtagonistInputHandler : MonoBehaviour {
		#region Fields
		protected Protagonist protagonist;
		protected Vector2 moveDelta;
		#endregion

		#region Life cycle
		protected void Start() {
			protagonist = GetComponent<Protagonist>();
		}

		protected void OnEnable() {
			Cursor.lockState = CursorLockMode.Locked;
		}

		protected void OnDisable() {
			Cursor.lockState = CursorLockMode.None;
		}

		protected void FixedUpdate() {
			Protagonist.MoveDelta(moveDelta * Time.fixedDeltaTime);
		}
		#endregion

		#region Internal accessors
		protected Protagonist Protagonist => protagonist;
		#endregion

		#region Handlers
		protected void OnMoveDelta(InputValue value) {
			moveDelta = value.Get<Vector2>();
		}

		protected void OnOrientDelta(InputValue value) {
			Protagonist.OrientDelta(value.Get<Vector2>());
		}

		protected void OnSetSprinting(InputValue value) {
			Protagonist.IsSprinting = value.Get<float>() > .5f;
		}
		#endregion
	}
}