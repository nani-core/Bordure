using UnityEngine;
using UnityEngine.InputSystem;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Protagonist))]
	public class ProtagonistInputHandler : MonoBehaviour {
		#region Fields
		protected Protagonist protagonist;
		protected PlayerInput playerInput;

		protected Vector3 moveVelocity;
		protected float floating = 0f, sinking = 0f;
		#endregion

		#region Life cycle
		protected void Start() {
			protagonist = GetComponent<Protagonist>();

			playerInput = gameObject.EnsureComponent<PlayerInput>();
			playerInput.notificationBehavior = PlayerNotifications.SendMessages;
			playerInput.actions.FindActionMap("Normal").Enable();

			SetInputMapActivity("Normal", true);
			UsesMovement = true;
			UsesOrientation = true;
		}

		protected void FixedUpdate() {
			if(!protagonist.IsInWater)
				moveVelocity.y = 0;
			else
				moveVelocity.y = floating - sinking;

			protagonist.MoveVelocity(moveVelocity);
		}

		protected void OnDisable() {
			// Reset cached input values on disabling, or the protagonist would keep
			// receiving false input when re-enabled.
			moveVelocity = Vector3.zero;
		}
		#endregion

		#region Interfaces
		public bool UsesMovement {
			get => GetInputMapActivity("Movement");
			set => SetInputMapActivity("Movement", value);
		}
		public bool UsesOrientation {
			get => GetInputMapActivity("Orientation");
			set => SetInputMapActivity("Orientation", value);
		}
		public bool UsesGrabbing {
			get => GetInputMapActivity("Grabbing");
			set => SetInputMapActivity("Grabbing", value);
		}
		#endregion

		#region Functions
		private InputActionMap FindInputMap(string mapName) {
			var map = playerInput.actions.FindActionMap(mapName);
			if(map == null)
				Debug.LogWarning($"Warning: Cannot find the protagonist input map \"{mapName}\".");
			return map;
		}

		private void SetInputMapActivity(string mapName, bool isActive) {
			var map = FindInputMap(mapName);
			if(map == null)
				return;
			if(isActive)
				map.Enable();
			else
				map.Disable();
		}

		private bool GetInputMapActivity(string mapName) {
			return FindInputMap(mapName)?.enabled ?? false;
		}
		#endregion

		#region Handlers
		// Normal

		protected void OnInteract() {
			protagonist.Interact();
		}

		protected void OnCheat() => protagonist?.Cheat();

		protected void OnLeave() {
			GameManager game = GameManager.Instance;
			if(game.CurrentSeat?.canLeaveManually ?? false)
				game.ProtagonistLeaveSeat();
		}

		// Movement

		protected void OnMoveVelocity(InputValue value) {
			var raw = value.Get<Vector2>();
			moveVelocity.x = raw.x;
			moveVelocity.z = raw.y;
		}

		protected void OnSetSprinting(InputValue value) {
			protagonist.IsSprinting = value.Get<float>() > .5f;
		}

		protected void OnJump(InputValue value) {
			float raw = value.Get<float>();
			floating = raw;
			if(raw > 0) {
				// Can jump in water.
				if(protagonist.IsOnGround)
					protagonist.Jump();
			}
		}

		protected void OnCrouch(InputValue value) {
			float raw = value.Get<float>();
			sinking = raw;
		}

		// Orientation

		protected void OnOrientDelta(InputValue value) {
			Vector2 raw = value.Get<Vector2>();
			if(!protagonist.GrabbingOrienting)
				protagonist.OrientDelta(raw);
			else
				protagonist.GrabbingOrientDelta(-raw.x);
		}

		// Grabbing

		protected void OnSetGrabbingOrienting(InputValue value) {
			bool raw = value.Get<float>() > .5f;
			protagonist.GrabbingOrienting = raw;
		}

		protected void OnResetGrabbingTransform() {
			protagonist.ResetGrabbingTransform();
		}
		#endregion
	}
}