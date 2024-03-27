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
			animator.SetBool("Counterclockwise", DetermineRollingDirection());
			animator.SetBool("Rolling", true);
			validator.enabled = false;
			isRolling = true;
		}

		public void EndRolling() {
			animator.SetBool("Rolling", false);
			validator.enabled = true;
			isRolling = false;
		}
		#endregion

		#region Function
		private bool DetermineRollingDirection() {
			var point = transform.worldToLocalMatrix.MultiplyPoint(GameManager.Instance.Protagonist.LookingPosition);
			Debug.Log(point);
			return point.y < 0;
		}
		#endregion
	}
}
