using UnityEngine;
using UnityEngine.InputSystem;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(Protagonist))]
	[RequireComponent(typeof(PlayerInput))]
	public class ProtagonistInputHandler : MonoBehaviour {
		#region Fields
		protected Protagonist protagonist;
		protected Vector2 moveVelocity;
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
			Protagonist.MoveVelocity(moveVelocity);
		}
		#endregion

		#region Internal accessors
		protected Protagonist Protagonist => protagonist;
		#endregion

		#region Handlers
		protected void OnMoveVelocity(InputValue value) {
			moveVelocity = value.Get<Vector2>();
		}

		protected void OnOrientDelta(InputValue value) {
			Protagonist.OrientDelta(value.Get<Vector2>());
		}

		protected void OnSetSprinting(InputValue value) {
			Protagonist.IsSprinting = value.Get<float>() > .5f;
		}

		protected void OnInteract() {
			Protagonist.Interact();
		}
		#endregion
	}
}