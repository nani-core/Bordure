using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace NaniCore.Stencil {
	public class Loopshape : MonoBehaviour {
		#region Serialized fields
		public bool oneTime;
		public UnityEvent onOpen = new();
		#endregion

		#region Fields
		private readonly HashSet<LoopshapeValidator> validValidators = new();
		#endregion

		#region Interfaces
		public bool IsValid => validValidators.Count > 0;

		public IEnumerable<LoopshapeValidator> ValidValidators => validValidators;

		public void Open() {
			onOpen?.Invoke();

			if(oneTime)
				enabled = false;
		}

		public void Hollow() {
			foreach(var validator in  validValidators) {
				OpticalValidator optical = validator as OpticalValidator;
				if(optical == null)
					continue;
				GameObject gastro = optical.gastro;
				this.EnsureComponent<HollowingManager>().Hollow(gastro);
				gastro.SetActive(false);
			}
		}

		public void OnValidatorUpdate(LoopshapeValidator validator) {
			if(validator.IsValid)
				validValidators.Add(validator);
			else
				validValidators.Remove(validator);
			validValidators.RemoveWhere(v => v == null);
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