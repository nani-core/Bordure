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
		/// Not yet to be implemented.
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

		#region Functions
		private void SetProgress(float progress) {
			if(openedTransform == null || closedTransform == null)
				return;
			target.transform.position = Vector3.Lerp(closedTransform.position, openedTransform.position, progress);
			target.transform.rotation = Quaternion.Slerp(closedTransform.rotation, openedTransform.rotation, progress);
			this.progress = progress;
		}

		public void ToggleOpeningState() {
			IsOpened = !IsOpened;
			onToggled.Invoke();
		}

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
				SetProgress(Mathf.Lerp(oldProgress, targetProgress, time / duration));
				yield return new WaitForFixedUpdate();
			}
			SetProgress(targetProgress);
		}
		#endregion

		#region Life cycle
		protected void Start() {
			SetProgress(IsOpened ? 1.0f : 0.0f);
		}
		#endregion
	}
}