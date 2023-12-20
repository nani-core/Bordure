using UnityEngine;
using UnityEngine.InputSystem;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(Protagonist))]
	public class ProtagonistInputHandler : MonoBehaviour {
		#region Fields
		protected Protagonist protagonist;
		protected PlayerInput playerInput;
		protected Vector2 moveVelocity;
		private Vector2 previousMoveVelocity;
		private InputActionMap grabbingActionMap;
		#endregion

		#region Life cycle
		protected void Start() {
			protagonist = GetComponent<Protagonist>();

			playerInput = gameObject.EnsureComponent<PlayerInput>();
			playerInput.actions = protagonist.Profile.inputActions;
			playerInput.notificationBehavior = PlayerNotifications.SendMessages;
			playerInput.actions.FindActionMap("Normal").Enable();

			grabbingActionMap = playerInput.actions.FindActionMap("Grabbing");
		}

		protected void OnEnable() {
			Cursor.lockState = CursorLockMode.Locked;
		}

		protected void OnDisable() {
			Cursor.lockState = CursorLockMode.None;
		}

		protected void FixedUpdate() {
			protagonist.MoveVelocity(moveVelocity);
		}
		#endregion

		#region Functions
		public void SetGrabbingActionEnabled(bool enabled) {
			if(enabled)
				grabbingActionMap.Enable();
			else
				grabbingActionMap.Disable();
		}
		#endregion

		#region Handlers
		protected void OnMoveVelocity(InputValue value) {
			moveVelocity = value.Get<Vector2>();
			if(previousMoveVelocity == Vector2.zero)
				protagonist.ResetSteppedDistance();
			previousMoveVelocity = moveVelocity;
		}

		protected void OnOrientDelta(InputValue value) {
			Vector2 raw = value.Get<Vector2>();
			if(!protagonist.GrabbingOrienting)
				protagonist.OrientDelta(raw);
			else
				protagonist.GrabbingOrientDelta(-raw.x);
		}

		protected void OnSetSprinting(InputValue value) {
			protagonist.IsSprinting = value.Get<float>() > .5f;
		}

		protected void OnJump() {
			protagonist.Jump();
		}

		protected void OnInteract() {
			protagonist.Interact();
		}

		protected void OnCheat() => protagonist?.Cheat();

		protected void OnSetGrabbingOrienting(InputValue value) {
			bool raw = value.Get<float>() > .5f;
			protagonist.GrabbingOrienting = raw;
		}
		#endregion
	}
}