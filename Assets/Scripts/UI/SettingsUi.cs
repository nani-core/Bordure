using UnityEngine;

namespace NaniCore.Bordure {
	public class SettingsUi : MonoBehaviour {
		#region Input message handler
		protected void OnPause() {
			GameManager.Instance.SettingsUiIsOpen = false;
		}
		#endregion
	}
}
