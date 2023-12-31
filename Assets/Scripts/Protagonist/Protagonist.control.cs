using UnityEngine;

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
		/// <summary>The 2D movement input during this frame, in operation space.</summary>
		private Vector2 bufferedMovementVelocity;
		/// <summary>The target velocity due to movement, in world space.</summary>
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

		private float MovingSpeed => IsSprinting ? Profile.sprintingSpeed : Profile.walkingSpeed;

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
		protected void InitializeControl() {
			if(Profile == null)
				return;

			ApplyGeometry();

			if(eye == null) {
				eye = transform.Find("Eye") ?? new GameObject("Eye").transform;
				eye.SetParent(transform, false);
			}
			eye.localPosition = Vector3.up * (Profile.height - Profile.eyeHanging);
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
			if(!IsOnGround)
				return;
			DealStepping(Time.fixedDeltaTime);

			UpdateWalkingAnimationState();
		}
		#endregion

		#region Functions
		private bool SweepTestGround(out RaycastHit hit, float distance, float backUpRatio = .5f, Vector3 offset = default) {
			var direction = -Upward;
			var hits = rigidbody.SweepTestAll(direction, distance, backUpRatio, offset, GameManager.Instance.GroundLayerMask);

			foreach(var candidate in hits) {
				// Check if the step is too sloped.
				float maxGroundingAngle = Mathf.Deg2Rad * Profile.maxGroundingAngle;
				float angle = Mathf.Deg2Rad * Vector3.Angle(-direction, candidate.normal);
				if(angle > maxGroundingAngle)
					continue;

				hit = candidate;
				return true;
			}

			hit = default;
			return false;
		}

		private void ApplyGeometry() {
			if(Profile == null)
				return;

			capsuleCollider = gameObject.EnsureComponent<CapsuleCollider>();
			capsuleCollider.height = Profile.height;
			capsuleCollider.center = Vector3.up * (Profile.height * .5f);
			capsuleCollider.radius = Profile.radius;

			rigidbody = gameObject.EnsureComponent<Rigidbody>();
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		}

		public void OrientDelta(Vector2 delta) {
			delta *= Profile.orientingSpeed;
			Azimuth += delta.x;
			Zenith += delta.y;
		}

		public void MoveVelocity(Vector2 vXy) {
			bufferedMovementVelocity += vXy;
			hasJustMoved = hasJustMoved || vXy.magnitude > .01f;
		}

		public void Jump() {
			if(!IsOnGround)
				return;

			var gravity = -Vector3.Dot(Physics.gravity, Upward);
			float speed = Mathf.Sqrt(2f * gravity * Profile.jumpingHeight);
			rigidbody.AddForce(Upward * speed, ForceMode.VelocityChange);
		}

		private void ValidateGround() {
			bool result = SweepTestGround(out RaycastHit hit, Profile.skinDepth);
			// Cannot jump when stepping on movable foundation.
			/*
			if(hit.rigidbody != null) {
				if(hit.rigidbody.velocity.magnitude > .01f)
					result = false;
			}
			*/
			isOnGround = result;
		}

		private void UpdateDesiredMovementVelocity(float deltaTime) {
			var bufferedDelta = bufferedMovementVelocity * deltaTime;
			bufferedMovementVelocity = Vector3.zero;
			desiredMovementVelocity = eye.transform.right * bufferedDelta.x + transform.forward * bufferedDelta.y;
			desiredMovementVelocity *= MovingSpeed / deltaTime;
			if(!IsOnGround) {
				desiredMovementVelocity *= Profile.midAirAttenuation;
			}
		}

		private void ApplyBufferedMovement(float deltaTime) {
			var targetVelocity = desiredMovementVelocity;

			var velocityDifference = targetVelocity - rigidbody.velocity;
			// Only taking horizontal movement into account.
			var force = velocityDifference.ProjectOntoPlane(Upward) * Profile.acceleration;
			rigidbody.AddForce(force, ForceMode.VelocityChange);
		}

		private void UpdateWalkingAnimationState() {
			isWalking = IsOnGround && hasJustMoved;
			hasJustMoved = false;
			animator?.SetBool("Walking", isWalking);
			animator?.SetBool("Sprinting", IsSprinting);
		}

		private void DealStepping(float deltaTime) {
			if(!IsOnGround)
				return;
			// Don't deal stepping when standing still.
			if(desiredMovementVelocity.magnitude < 1e-1f)
				return;
			
			// Prepare basic values.
			// Don't use real horizontal velocity, or the cast won't succeed when touching walls.
			Vector3 vx = desiredMovementVelocity.ProjectOntoPlane(Upward);
			Vector3 vy = rigidbody.velocity.ProjectOntoAxis(Upward);

			// Calculate the step horizontal offset.
			Vector3 deltaX = vx * Mathf.Max(deltaTime, .1f);
			// For greater moving speed, the offset is amplified to hitting the staircase.
			if(deltaX.magnitude < Profile.stepDetectionDistance)
				deltaX = deltaX.normalized * Profile.stepDetectionDistance;

			// Perform the sweep cast and calculate vertical offset.
			var isHit = SweepTestGround(out RaycastHit hit, Profile.stepHeight * 2, .5f, deltaX);
			if(!isHit)
				return;
			Vector3 deltaY = (hit.point - rigidbody.position).ProjectOntoAxis(Upward);

			// Invalidate downward steppings.
			if(Vector3.Dot(deltaY, Upward) < 1e-1f)
				return;

			// Calculate expected vertical velocity.
			Vector3 expectedVy = Upward * Mathf.Sqrt(2f * deltaY.magnitude * Physics.gravity.magnitude);
			Vector3 deltaVy = expectedVy - vy;

			// Apply vertical velocity.
			rigidbody.AddForce(deltaVy, ForceMode.VelocityChange);
		}
		#endregion
	}
}