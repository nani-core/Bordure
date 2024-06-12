using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Collider))]
	public class InputGuidanceVolume : MonoBehaviour {
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

		protected void OnTriggerEnter(Collider other) {
			if(other.gameObject != GameManager.Instance.Protagonist.gameObject)
				return;

			SendMessage("Show", SendMessageOptions.DontRequireReceiver);
		}

		protected void OnTriggerExit(Collider other) {
			if(other.gameObject != GameManager.Instance.Protagonist.gameObject)
				return;

			SendMessage("Hide", SendMessageOptions.DontRequireReceiver);
		}
		#endregion

		#region Fields
		private bool isVisible = false;
		#endregion

		#region Functions
		private void SetVisibility(bool value) {
			if(isVisible == value)
				return;
			if(isVisible = value)
				GameManager.Instance.ShowGuidanceList(keys);
			else
				GameManager.Instance.HideGuidanceList(keys);
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
