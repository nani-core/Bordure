using UnityEngine;

namespace NaniCore.Bordure {
	[CreateAssetMenu(menuName = "Nani Core/Input Guidance Sheet")]
	public class InputGuidanceSheet : ScriptableObject {
		public InputGuidanceEntry[] guidances;
	}
}
