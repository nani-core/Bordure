using UnityEngine;
using UnityEngine.UI;

namespace NaniCore.Bordure {
	public class SettingsUi : MonoBehaviour {
		#region Serialized fields
		public Slider mouseSensivity;
		#endregion

		#region Life cycle
		protected void OnEnable() {
			mouseSensivity.value = Game.MouseSensitivityGainInExponent;
		}
		#endregion

		#region Input message handler
		protected void OnPause() {
			Game.SettingsUiIsOpen = false;
		}
		#endregion

		#region Interfaces
		public void OnMouseSensitivityValueChanged() {
			Game.MouseSensitivityGainInExponent = mouseSensivity.value;
		}
		#endregion

		#region Functions
		private GameManager Game => GameManager.Instance;
		#endregion
	}
}
