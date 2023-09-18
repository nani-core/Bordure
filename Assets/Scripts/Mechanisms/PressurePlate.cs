using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(Collider))]
	public class PressurePlate : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Transform plate;
		[SerializeField][Min(0)] private float sinkingDistance = .1f;
		[SerializeField] private List<GameObject> targets = new List<GameObject>();

		[SerializeField] private AudioClip onPressedSound;
		[SerializeField] private UnityEvent onPressed;
		[SerializeField] private AudioClip onReleasedSound;
		[SerializeField] private UnityEvent onReleased;
		#endregion

		#region Fields
		private bool pressed = false;
		private HashSet<GameObject> enteredTargets = new HashSet<GameObject>();
		private Vector3 startingPosition;
		#endregion

		#region Properties
		public bool Pressed {
			get => pressed;
			set {
				if(pressed == value)
					return;
				if(pressed = value) {
					plate.localPosition = startingPosition + Vector3.down * sinkingDistance;
					StartCoroutine(AudioUtility.PlayOneShotAtCoroutine(onPressedSound, transform.position, transform));
					onPressed?.Invoke();
				}
				else {
					plate.localPosition = startingPosition;
					StartCoroutine(AudioUtility.PlayOneShotAtCoroutine(onReleasedSound, transform.position, transform));
					onReleased?.Invoke();
				}
			}
		}
		#endregion

		#region Functions
		private void UpdatePressingState() {
			Pressed = enteredTargets.Count > 0;
		}

		private static bool IsTargetInvalidated(GameObject target) {
			return target == null || !target.activeInHierarchy;
		}
		#endregion

		#region Life cycle
		protected void Start() {
			var collider = GetComponent<Collider>();
			if(collider != null) {
				collider.isTrigger = true;
			}

			startingPosition = plate.localPosition;
		}

		protected void Update() {
			if(enteredTargets.Any(IsTargetInvalidated)) {
				enteredTargets.RemoveWhere(IsTargetInvalidated);
				UpdatePressingState();
			}
		}

		protected void OnTriggerEnter(Collider other) {
			if(targets.Contains(other.gameObject)) {
				enteredTargets.Add(other.gameObject);
				UpdatePressingState();
			}
		}

		protected void OnTriggerExit(Collider other) {
			if(enteredTargets.Contains(other.gameObject)) {
				enteredTargets.Remove(other.gameObject);
				UpdatePressingState();
			}
		}
		#endregion
	}
}