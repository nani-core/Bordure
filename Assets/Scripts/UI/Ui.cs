using UnityEngine;

namespace NaniCore.Bordure {
	public class Ui : MonoBehaviour {
		#region Interfaces
		public virtual void OnEnter() {
		}

		public virtual void OnExit() {
		}

		public virtual void OnShow() {
			gameObject.SetActive(true);
		}

		public virtual void OnHide() {
			gameObject.SetActive(false);
		}
		#endregion
	}
}