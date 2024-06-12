using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		#endregion
	}
}
