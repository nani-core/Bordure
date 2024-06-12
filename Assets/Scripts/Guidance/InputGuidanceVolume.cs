using UnityEngine;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Collider))]
	public class InputGuidanceVolume : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private string[] keys;
		#endregion

		#region Life cycle
		protected void Start() {
			GameManager.Instance.onProtagonistDisabled += Hide;
		}

		protected void OnDestroy() {
			GameManager.Instance.onProtagonistDisabled -= Hide;
		}

		protected void OnDisable() {
			Showing = false;
		}

		protected void OnTriggerEnter(Collider other) {
			if(other.gameObject != GameManager.Instance.Protagonist.gameObject)
				return;

			Showing = true;
		}

		protected void OnTriggerExit(Collider other) {
			if(other.gameObject != GameManager.Instance.Protagonist.gameObject)
				return;

			Showing = false;
			gameObject.SetActive(false);
		}
		#endregion

		#region Fields
		private bool showing = false;
		#endregion

		#region Interfaces
		public bool Showing {
			get => showing;
			set {
				if(showing == value)
					return;
				if(showing = value) {
					GameManager.Instance.ShowGuidanceList(keys);
					SendMessage("OnShown", SendMessageOptions.DontRequireReceiver);
				}
				else {
					GameManager.Instance.HideGuidanceList(keys);
					SendMessage("OnHidden", SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		public void Show() {
			Showing = false;
		}

		public void Hide() {
			Showing = false;
		}
		#endregion
	}
}
