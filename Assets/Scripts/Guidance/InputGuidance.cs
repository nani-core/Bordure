using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	public class InputGuidance : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private string[] keys;
		public UnityEvent onShown, onHidden;
		#endregion

		#region Life cycle
		protected void Start() {
			GameManager.Instance.onProtagonistDisabled += Hide;
		}

		protected void OnDestroy() {
			GameManager.Instance.onProtagonistDisabled -= Hide;
		}

		protected void OnDisable() {
			SetVisibility(false);
		}
		#endregion

		#region Fields
		private bool isVisible = false;
		#endregion

		#region Functions
		private void SetVisibility(bool value) {
			if(isVisible == value)
				return;
			if(isVisible = value) {
				GameManager.Instance.ShowGuidanceList(keys);
				SendMessage("OnShown", SendMessageOptions.DontRequireReceiver);
			}
			else {
				GameManager.Instance.HideGuidanceList(keys);
				SendMessage("OnHidden", SendMessageOptions.DontRequireReceiver);
			}
		}
		#endregion

		#region Interfaces
		public void Show() {
			SetVisibility(true);
			onShown?.Invoke();
		}

		public void Hide() {
			SetVisibility(false);
			onHidden?.Invoke();
		}
		#endregion
	}
}
