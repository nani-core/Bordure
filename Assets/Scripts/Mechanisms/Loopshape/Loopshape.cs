using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	public class Loopshape : MonoBehaviour {
		#region Serialized fields
		public bool oneTime;
		public UnityEvent onOpen = new();
		public UnityEvent onValidated = new();
		public UnityEvent onInvalidated = new();
		#endregion

		#region Fields
		private readonly HashSet<LoopshapeValidator> validators = new();
		private bool wasValid = false;
		#endregion

		#region Interfaces
		public bool IsValid => isActiveAndEnabled && ValidValidators.Count() > 0;

		public IEnumerable<LoopshapeValidator> ValidValidators => validators.Where(validator => validator.IsValid);

		public void Open() {
			Debug.Log($"{this} is opened.", this);
			onOpen?.Invoke();

			if(oneTime)
				enabled = false;
		}

		/// <summary>
		/// This is a delegating function to automatically pass-in the gastro object.
		/// The level designer could also manually pass it in.
		/// </summary>
		public void Hollow() {
			StartCoroutine(HollowCoroutine());
		}

		public void RegisterValidator(LoopshapeValidator validator) {
			validators.Add(validator);
		}

		public void OnValidatorUpdate(LoopshapeValidator validator) {
			// Update valid state.
			if(wasValid != IsValid) {
				if(wasValid = IsValid)
					onValidated.Invoke();
				else
					onInvalidated.Invoke();
			}
		}
		#endregion

		#region Functions
		private System.Collections.IEnumerator HollowCoroutine() {
			// Delay hollowing by one frame, to let other components (for
			// example, `ModelHighlighter`) to finish their jobs.
			yield return new WaitForEndOfFrame();
			foreach(var validator in validators) {
				OpticalValidator optical = validator as OpticalValidator;
				if(optical == null)
					continue;
				GameObject gastro = optical.gastro;
				this.EnsureComponent<HollowingManager>().Hollow(gastro);
				gastro.SetActive(false);
			}
		}
		#endregion

		#region Life cycle
		protected void Start() {
			if(GameManager.Instance != null)
				GameManager.Instance.SendMessage("OnLoopShapeCreated", this);
		}

		protected void OnDestroy() {
			if(GameManager.Instance != null)
				GameManager.Instance.SendMessage("OnLoopShapeDestroyed", this);
		}
		#endregion
	}
}