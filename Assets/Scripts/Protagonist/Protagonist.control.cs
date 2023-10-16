using System.Collections;
using UnityEngine;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Fields
		private Transform eye;
		private CapsuleCollider capsuleCollider;
		private new Rigidbody rigidbody;
		private bool isOnGround = false;
		private bool isRunning = false;
		private bool wasJustJumping = false;
		private float steppedDistance = 0;
		private Vector2 bufferedMovementDelta, bufferedMovementVelocity;
		private Vector3 desiredMovementVelocity;
		#endregion

		#region Properties
		public Transform Eye => eye;
		public Vector3 Upward => transform.up;

		public bool IsOnGround => isOnGround;

		public bool IsSprinting {
			get => isRunning;
			set => isRunning = value;
		}

		private float MovingSpeed => IsSprinting ? profile.sprintingSpeed : profile.walkingSpeed;

#pragma warning disable IDE0052 // Remove unread private members
		private float SteppedDistance {
			get => steppedDistance;
			set {
				steppedDistance = value;
				if(profile.stepDistance <= 0)
					return;
				if(steppedDistance < 0 || steppedDistance > profile.stepDistance) {
					steppedDistance = steppedDistance.Mod(profile.stepDistance);
					PlayFootstepSound();
				}
			}
		}
#pragma warning restore IDE0052 // Remove unread private members

		/// <summary>
		/// What direction is the protagonist looking at, in rad.
		/// </summary>
		/// <remarks>
		/// Z+ is the starting position, rotating clockwise.
		/// </remarks>
		public float Azimuth {
			get {
				Vector3 forward = transform.forward;
				return Mathf.Atan2(forward.x, forward.z);
			}
			set {
				float degree = value * 180 / Mathf.PI;
				// Needs to be refined to support unusual cases.
				transform.rotation = Quaternion.Euler(0, degree, 0);
			}
		}

		/// <summary>
		/// How high is the protagonist looking at, in rad.
		/// </summary>
		/// <remarks>
		/// Above the XZ plane is positive; below is negative.
		/// </remarks>
		public float Zenith {
			get {
				Vector3 forward = eye.forward;
				float y = forward.y;
				forward.y = 0;
				return Mathf.Atan2(y, forward.magnitude);
			}
			set {
				float degree = value * 180 / Mathf.PI;
				degree = Mathf.Clamp(degree, -90, 90);
				eye.localRotation = Quaternion.Euler(-degree, 0, 0);
			}
		}
		#endregion

		#region Life cycle
		protected void StartControl() {
			if(Profile == null)
				return;

			ApplyGeometry();

			if(eye == null) {
				eye = new GameObject("Eye").transform;
				eye.SetParent(transform, false);
			}
			eye.localPosition = Vector3.up * (profile.height - profile.eyeHanging);
			eye.localRotation = Quaternion.identity;
			eye.localScale = Vector3.one;
		}

#if UNITY_EDITOR
		protected void ValidateControl() {
			ApplyGeometry();
		}
#endif

		protected void FixedUpdateControl() {
			ValidateGround();
			UpdateDesiredMovementVelocity(Time.fixedDeltaTime);
			DealBufferedMovement(Time.fixedDeltaTime);
		}

		protected void LateUpdateControl() {
			DealStepping();
		}
		#endregion

		#region Functions
		private void ApplyGeometry() {
			if(Profile == null)
				return;

			capsuleCollider = gameObject.EnsureComponent<CapsuleCollider>();
			capsuleCollider.height = profile.height;
			capsuleCollider.center = Vector3.up * (profile.height * .5f);
			capsuleCollider.radius = profile.radius;

			rigidbody = gameObject.EnsureComponent<Rigidbody>();
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		}

		private bool SweepTest(Vector3 direction, out RaycastHit hitInfo, float distance, float backupRatio = 0) {
			direction.Normalize();
			var originalPos = rigidbody.position;
			rigidbody.position -= direction * distance * backupRatio;
			bool result = rigidbody.SweepTest(direction, out hitInfo, distance);
			if(result) {
				int shouldExclude = hitInfo.collider.gameObject.layer & rigidbody.excludeLayers;
				if(shouldExclude != 0)
					result = false;
			}
			rigidbody.position = originalPos;
			return result;
		}

		public void OrientDelta(Vector2 delta) {
			delta *= profile.orientingSpeed;
			Azimuth += delta.x;
			Zenith += delta.y;
		}

		public void MoveVelocity(Vector2 vXy) {
			bufferedMovementVelocity += vXy;
		}

		public void MoveDelta(Vector2 dXy) {
			bufferedMovementDelta += dXy;
		}

		public void Jump() {
			if(!IsOnGround)
				return;

			var gravity = -Vector3.Dot(Physics.gravity, Upward);
			float speed = Mathf.Sqrt(2f * gravity * profile.jumpingHeight);
			rigidbody.AddForce(Upward * speed, ForceMode.VelocityChange);
			StartCoroutine(JumpCoroutine());
		}

		private void ValidateGround() {
			bool result = SweepTest(Physics.gravity, out RaycastHit hitInfo, profile.skinDepth, .5f);
			var hitRb = hitInfo.rigidbody;
			if(hitRb != null) {
				if(hitRb.velocity.magnitude > .01f)
					result = false;
			}
			isOnGround = result;
		}

		private IEnumerator JumpCoroutine() {
			wasJustJumping = true;
			yield return new WaitForSeconds(.1f);
			wasJustJumping = false;
		}

		private void UpdateDesiredMovementVelocity(float deltaTime) {
			var bufferedDelta = bufferedMovementDelta + bufferedMovementVelocity * deltaTime;
			bufferedMovementDelta = Vector3.zero;
			bufferedMovementVelocity = Vector3.zero;
			desiredMovementVelocity = eye.transform.right * bufferedDelta.x + transform.forward * bufferedDelta.y;
			desiredMovementVelocity *= MovingSpeed / deltaTime;
			if(!IsOnGround)
				desiredMovementVelocity = Vector3.zero;
		}

		private void DealBufferedMovement(float deltaTime) {
			if(!IsOnGround)
				return;

			var targetVelocity = desiredMovementVelocity;

			var desiredPositionChange = desiredMovementVelocity * deltaTime;
			if(SweepTest(desiredMovementVelocity, out RaycastHit hitInfo, desiredPositionChange.magnitude, 0)) {
				targetVelocity = targetVelocity.normalized * Vector3.Dot(hitInfo.point - rigidbody.position, desiredMovementVelocity.normalized);
			}

			var velocityDifference = targetVelocity - rigidbody.velocity;
			rigidbody.AddForce(velocityDifference.ProjectOntoPlane(Upward) * profile.acceleration, ForceMode.VelocityChange);
		}

		private void DealStepping() {
			if(wasJustJumping || desiredMovementVelocity.magnitude == 0f)
				return;

			var originalPosition = rigidbody.position;
			var offset = desiredMovementVelocity.normalized * profile.skinDepth;
			rigidbody.position += offset;
			var isHit = SweepTest(-Upward, out RaycastHit stepHitInfo, profile.stepHeight * 2f, .5f);
			rigidbody.position = originalPosition;
			if(!isHit)
				return;
			var deltaY = Vector3.Dot(stepHitInfo.point - rigidbody.position, Upward);
			if(Mathf.Abs(deltaY) < .1f)
				return;
			// Teleport to desired position.
			var desiredPosition = originalPosition + (stepHitInfo.point - offset - originalPosition).ProjectOntoAxis(Upward);
			rigidbody.MovePosition(desiredPosition);
			// Grant helper velocity.
			var minimumHelperVelocity = desiredMovementVelocity.normalized * (MovingSpeed * .5f);
			var helperSpeed = Vector3.Dot(minimumHelperVelocity, desiredMovementVelocity.normalized);
			helperSpeed = Mathf.Max(0, helperSpeed);
			var trimmedHelperVelocity = desiredMovementVelocity.normalized * helperSpeed;
			rigidbody.AddForce(trimmedHelperVelocity, ForceMode.VelocityChange);
			rigidbody.velocity = rigidbody.velocity.ProjectOntoPlane(Upward);
		}
		#endregion
	}
}