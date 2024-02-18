using UnityEngine;

namespace NaniCore.Stencil {
	public abstract class LoopshapeValidator : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Loopshape loopshape;
		#endregion

		#region Fields
		private bool isValid = false;
		#endregion

		#region Interfaces
		public bool IsValid {
			get => isValid;
			set {
				if(isValid == value)
					return;
				isValid = value;
				ChangeValidatedState();
			}
		}

		public Loopshape Loopshape {
			get {
				if(loopshape == null)
					loopshape = GetComponent<Loopshape>();
				return loopshape;
			}
		}
		#endregion

		#region Functions
		protected abstract bool Validate();

		private void ChangeValidatedState() {
			Loopshape.OnValidatorUpdate(this);
		}
		#endregion

		#region Life cycle
		#if UNITY_EDITOR
		protected void OnValidate() {
			if(Application.isPlaying)
				return;

			if(loopshape == null)
				loopshape = GetComponent<Loopshape>();
		}
		#endif

		protected void Start() {
			ChangeValidatedState();
		}

		protected void Update() {
			if(Loopshape.isActiveAndEnabled)
				IsValid = Validate();
			else
				IsValid = false;
		}
		#endregion
	}
}