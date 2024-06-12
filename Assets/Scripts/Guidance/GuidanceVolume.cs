using UnityEngine;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Collider))]
	public class GuidanceVolume : MonoBehaviour {
		#region Life cycle
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
	}
}
