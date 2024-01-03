using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

namespace NaniCore {
	/// <summary>
	/// Double-throw carrier.
	/// Used to toggle and transit a scene object between two anchors.
	/// </summary>
	public partial class DtCarrier : Carrier {
		#region Serialized fields
		/// <remarks>Not yet implemented.</remarks>
		[SerializeField] private AudioClip movingSound;
		[SerializeField] private AudioClip onClosedSound;
		[SerializeField] private Transform closedTransform;
		[SerializeField] private AudioClip onOpenedSound;
		[SerializeField] private Transform openedTransform;
		[SerializeField] private bool isOpened = false;
		[SerializeField][Min(0)] private float openingDuration = 1f;

		[SerializeField] private UnityEvent onOpened;
		[SerializeField] private UnityEvent onClosed;
		[SerializeField] private UnityEvent onToggled;
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

				Vector3 newPosition = Vector3.Lerp(closedTransform.position, openedTransform.position, progress);
				Quaternion newOrientation = Quaternion.Slerp(closedTransform.rotation, openedTransform.rotation, progress);

				if(Rigidbody != null) {
					Rigidbody.MovePosition(newPosition);
					Rigidbody.MoveRotation(newOrientation);
				}
				else {
					Target.transform.position = newPosition;
					Target.transform.rotation = newOrientation;
				}
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
			yield return EaseProgressCoroutine(targetOpened ? 1f : 0f, duration);
			if(targetOpened == false && isOpened == true)
				StartCoroutine(AudioUtility.PlayOneShotAtCoroutine(onClosedSound, transform.position, transform));
			isOpened = targetOpened;
			if(isOpened)
				onOpened.Invoke();
			else
				onClosed.Invoke();
		}

		private IEnumerator EaseProgressCoroutine(float targetProgress, float duration) {
			float oldProgress = progress;
			duration *= Mathf.Abs(oldProgress - targetProgress);
			float startTime = Time.time;
			for(float time; (time = Time.time - startTime) < duration;) {
				Progress = Mathf.Lerp(oldProgress, targetProgress, time / duration);
				yield return new WaitForFixedUpdate();
			}
			Progress = targetProgress;
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