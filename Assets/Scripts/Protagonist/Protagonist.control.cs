using UnityEngine;
using System.Collections;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private new Camera camera;
		[SerializeField] private Animator animator;
		[SerializeField] private Transform eye;
		#endregion

		#region Fields
		private CapsuleCollider capsuleCollider;
		private new Rigidbody rigidbody;
		private bool isOnGround = false;
		private bool hasJustMoved = false;
		private bool isWalking = false;
		private bool isSprinting = false;
		private bool isJumping = false;
		private Vector2 bufferedMovementDelta, bufferedMovementVelocity;
		private Vector3 desiredMovementVelocity;
		#endregion

		#region Properties
		public Transform Eye => eye;
		public Camera Camera => camera;
		public Vector3 Upward => transform.up;

		public bool IsOnGround => isOnGround;
		public bool IsWalking => isWalking;

		public bool IsSprinting {
			get => isSprinting;
			set => isSprinting = value;
		}
		public bool IsJumping => isJumping;

		private float MovingSpeed => IsSprinting ? profile.sprintingSpeed : profile.walkingSpeed;

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

		#region Functions
		private bool SweepTestGround(out RaycastHit hit, float distance, float backUpRatio = .5f, Vector3 offset = default) {
			return rigidbody.SweepTestEx(-Upward, out hit, distance, backUpRatio, offset, GameManager.Instance.GroundLayerMask);
		}
		#endregion

		#region Life cycle
		protected void StartControl() {
			if(Profile == null)
				return;

			ApplyGeometry();

			if(eye == null) {
				eye = transform.Find("Eye") ?? new GameObject("Eye").transform;
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
			ApplyBufferedMovement(Time.fixedDeltaTime);
			UpdateWalkingState();
		}

		protected void LateUpdateControl() {
			if(isJumping || desiredMovementVelocity.magnitude == 0f)
				return;

			var originalPosition = rigidbody.position;
			var offset = desiredMovementVelocity.normalized * profile.skinDepth;
			rigidbody.position += offset;
			var isHit = SweepTestGround(out RaycastHit stepHit, profile.stepHeight * 2f);
			rigidbody.position = originalPosition;
			if(!isHit)
				return;
			var deltaY = Vector3.Dot(stepHit.point - rigidbody.position, Upward);
			if(Mathf.Abs(deltaY) < .1f)
				return;
			// Teleport to desired position.
			var desiredPosition = originalPosition + (stepHit.point - offset - originalPosition).ProjectOntoAxis(Upward);
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

		public void OrientDelta(Vector2 delta) {
			delta *= profile.orientingSpeed;
			Azimuth += delta.x;
			Zenith += delta.y;
		}

		public void MoveVelocity(Vector2 vXy) {
			bufferedMovementVelocity += vXy;
			hasJustMoved = hasJustMoved || vXy.magnitude > .01f;
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
			bool result = SweepTestGround(out RaycastHit hit, profile.skinDepth);
			// Cannot jump when stepping on movable foundation.
			/*
			if(hit.rigidbody != null) {
				if(hit.rigidbody.velocity.magnitude > .01f)
					result = false;
			}
			*/
			isOnGround = result;
		}

		private IEnumerator JumpCoroutine() {
			isJumping = true;
			yield return new WaitForSeconds(.1f);
			yield return new WaitUntil(() => IsOnGround);
			isJumping = false;
		}

		private void UpdateDesiredMovementVelocity(float deltaTime) {
			var bufferedDelta = bufferedMovementDelta + bufferedMovementVelocity * deltaTime;
			bufferedMovementDelta = Vector3.zero;
			bufferedMovementVelocity = Vector3.zero;
			desiredMovementVelocity = eye.transform.right * bufferedDelta.x + transform.forward * bufferedDelta.y;
			desiredMovementVelocity *= MovingSpeed / deltaTime;
			if(!IsOnGround) {
				desiredMovementVelocity *= profile.midAirAttenuation;
			}
		}

		private void ApplyBufferedMovement(float deltaTime) {
			var targetVelocity = desiredMovementVelocity;

			var velocityDifference = targetVelocity - rigidbody.velocity;
			// Only taking horizontal movement into account.
			var force = velocityDifference.ProjectOntoPlane(Upward) * profile.acceleration;
			rigidbody.AddForce(force, ForceMode.VelocityChange);
		}

		private void UpdateWalkingState() {
			isWalking = IsOnGround && hasJustMoved;
			hasJustMoved = false;
			animator?.SetBool("Walking", isWalking);
			animator?.SetBool("Sprinting", IsSprinting);
		}
		#endregion
	}
}