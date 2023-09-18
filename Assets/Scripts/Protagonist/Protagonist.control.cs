using System.Collections.Generic;
using UnityEngine;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(CharacterController))]
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[Header("Geometry")]
		[SerializeField][Min(0)] private float height;
		[SerializeField][Min(0)] private float radius;
		[SerializeField] private Transform eye;
		[SerializeField][Min(0)] private float eyeHanging;

		[Header("Physics")]
		[SerializeField] private CharacterController characterController;

		[Header("Control")]
		[SerializeField][Min(0)] private float walkingSpeed;
		[SerializeField][Min(0)] private float sprintingSpeed;
		[SerializeField][Min(0)] private float stepDistance;
		[SerializeField] private AudioSource footAudioSource;
		[SerializeField] private List<AudioClip> stepAudioClips = new List<AudioClip>();
		[SerializeField][Min(0)] private float orientingSpeed;
		#endregion

		#region Fields
		private bool isRunning = false;
		private float steppedDistance = 0;
		private Vector3 accumulatedMovementVelocity;
		#endregion

		#region Properties
		public Transform Eye => eye;
		public Vector3 EyeOffset => transform.localToWorldMatrix.MultiplyVector(eye.localPosition);

		public bool IsSprinting {
			get => isRunning;
			set => isRunning = value;
		}

		private float MovingSpeed => IsSprinting ? sprintingSpeed : walkingSpeed;

#pragma warning disable IDE0052 // Remove unread private members
		private float SteppedDistance {
			get => steppedDistance;
			set {
				steppedDistance = value;
				if(stepDistance <= 0)
					return;
				if(steppedDistance < 0 || steppedDistance > stepDistance) {
					steppedDistance = steppedDistance.Mod(stepDistance);
					footAudioSource.PlayOneShot(stepAudioClips.PickRandom());
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
#if UNITY_EDITOR
		private void ValidateControl() {
			characterController = GetComponent<CharacterController>();
			if(characterController != null) {
				characterController.height = height;
				characterController.center = Vector3.up * (height * .5f);
				characterController.radius = radius;
			}

			if(eye != null) {
				eye.localPosition = Vector3.up * (height - eyeHanging);
			}
		}
#endif

		protected void FixedUpdate() {
			if(characterController.SimpleMove(accumulatedMovementVelocity)) {
				SteppedDistance += accumulatedMovementVelocity.magnitude * Time.fixedDeltaTime;
			}
		}
		#endregion

		#region Functions
		public void OrientDelta(Vector2 delta) {
			delta *= orientingSpeed;
			Azimuth += delta.x;
			Zenith += delta.y;
		}

		public void MoveVelocity(Vector2 vXy) {
			vXy *= MovingSpeed;
			accumulatedMovementVelocity = eye.transform.right * vXy.x + transform.forward * vXy.y;
		}

		public void MoveDelta(Vector3 delta) {
			characterController.SimpleMove(delta);
			SteppedDistance += delta.magnitude;
		}
		#endregion
	}
}