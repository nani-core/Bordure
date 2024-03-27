using UnityEngine;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(FocusValidator))]
	public class BinLid : MonoBehaviour {
		#region Fields
		[SerializeField] private FocusValidator validator;
		[SerializeField] private Animator animator;
		
		private bool isRolling = false;
		#endregion

		#region Interfaces
		public void StartRolling() {
			if(isRolling) {
				Debug.LogWarning($"{this} is already rolling. Cancelling new rolling request.");
				return;
			}
			animator.Play("Rolling");
			validator.enabled = false;
			isRolling = true;
		}

		public void EndRolling() {
			validator.enabled = true;
			isRolling = false;
		}
		#endregion
	}
}
