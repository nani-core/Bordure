using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace NaniCore {
	/// <summary>
	/// Double-throw carrier.
	/// Used to toggle and transit a scene object between two anchors.
	/// </summary>
	public partial class DtCarrier : Carrier {
		#region Serialized fields
		/// <remarks>Not yet implemented.</remarks>
		public AudioClip movingSound;
		public AudioClip onClosedSound;
		public Transform closedTransform;
		public AudioClip onOpenedSound;
		public Transform openedTransform;
		/// <summary>
		/// Whether or not the carrier is opened before SPAWNING.
		/// </summary>
		public bool isOpened = false;
		[Min(0)] public float openingDuration = 1.0f;
		public float easingFactor = 0.0f;

		public UnityEvent onOpened;
		public UnityEvent onClosed;
		public UnityEvent onToggled;
		#endregion

		#region Fields
		private float progress = 0f;
		private Coroutine movementCoroutine;
		#endregion

		#region Properties
		public bool IsOpened {
			get => isOpened;
			set {
				if(movementCoroutine != null) {
					StopCoroutine(movementCoroutine);
					movementCoroutine = null;
				}
				movementCoroutine = StartCoroutine(SetOpeningStateCoroutine(value, openingDuration));
			}
		}

		public float Duration {
			get => openingDuration;
			set => openingDuration = value;
		}
		#endregion

		#region Interface
		public float Progress {
			get => progress;
			set {
				if(Target == null)
					return;
				if(openedTransform == null || closedTransform == null)
					return;

				progress = value;

				Target.Lerp(closedTransform, openedTransform, progress);
			}
		}

		public void ToggleOpeningState() {
			IsOpened = !IsOpened;
			onToggled.Invoke();
		}
		#endregion

		#region Functions
		private IEnumerator SetOpeningStateCoroutine(bool targetOpened, float duration) {
			if(targetOpened == true && isOpened == false)
				StartCoroutine(AudioUtility.PlayOneShotAtCoroutine(onOpenedSound, transform.position, transform));

			float startProgress = progress, endProgress = targetOpened ? 1.0f : 0.0f;
			duration *= Mathf.Abs(startProgress - endProgress);
			yield return MathUtility.ProgressCoroutine(
				duration,
				progress => Progress = Mathf.Lerp(startProgress, endProgress, progress),
				easingFactor
			);
			
			if(targetOpened == false && isOpened == true)
				StartCoroutine(AudioUtility.PlayOneShotAtCoroutine(onClosedSound, transform.position, transform));
			isOpened = targetOpened;
			if(isOpened)
				onOpened.Invoke();
			else
				onClosed.Invoke();
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			base.Start();

			if(Rigidbody != null)
				Rigidbody.isKinematic = true;
			Progress = IsOpened ? 1.0f : 0.0f;
		}
		#endregion
	}
}