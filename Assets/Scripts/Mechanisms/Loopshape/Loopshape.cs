using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Stencil {
	public class Loopshape : MonoBehaviour {
		#region Serialized fields
		[SerializeField] public UnityEvent onOpen;
		#endregion

		#region Fields
		private LoopshapeValidator validator;
		#endregion

		#region Interfaces
		public bool IsValid {
			get {
				if(validator == null || !isActiveAndEnabled)
					return false;
				if(validator.isActiveAndEnabled && validator.IsValid)
					return true;
				return false;
			}
		}

		public void Open() {
			onOpen?.Invoke();
		}

		public void Hollow() {
			switch(validator) {
				case OpticalValidator opt:
					GameObject gastro = opt.gastro;
					this.EnsureComponent<HollowingManager>().Hollow(gastro);
					gastro.SetActive(false);
					break;
				default:
					Debug.LogWarning($"This loopshape doesn't have a hollowable validator type.", this);
					return;
			}

		}
		#endregion

		#region Life cycle
		protected void Start() {
			validator = GetComponentInChildren<LoopshapeValidator>(true);

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