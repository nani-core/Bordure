using UnityEngine.UI;

namespace NaniCore.Bordure {
	public class SettingsMenu : Menu {
		#region Serialized fields
		public Slider mouseSensivity;
		#endregion

		#region Life cycle
		protected void OnEnable() {
			mouseSensivity.value = GameManager.Instance.MouseSensitivityGainInExponent;
		}
		#endregion

		#region Interfaces
		public void OnMouseSensitivityValueChanged() {
			GameManager.Instance.MouseSensitivityGainInExponent = mouseSensivity.value;
		}
		#endregion
	}
}
