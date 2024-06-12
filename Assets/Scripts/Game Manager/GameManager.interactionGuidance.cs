using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Serialized fields
		[SerializeField] private InputGuidanceManager inputGuidance;
		#endregion

		#region Interfaces
		public void ShowGuidance(string key) {
			inputGuidance.ShowByKey(key);
		}

		public void HideGuidance(string key) {
			inputGuidance.HideByKey(key);
		}

		public void ShowGuidanceList(params string[] keys) {
			foreach(var key in keys)
				ShowGuidance(key);
		}

		public void HideGuidanceList(params string[] keys) {
			foreach(var key in keys)
				HideGuidance(key);
		}
		#endregion
	}
}
