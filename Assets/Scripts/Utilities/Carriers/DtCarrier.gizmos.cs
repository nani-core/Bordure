#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore.Loopool {
	public partial class DtCarrier : MonoBehaviour {
		#region Life cycle
		protected void OnDrawGizmos() {
			if(door != null) {
				if(closedTransform != null) {
					GizmosUtility.SetColor(Color.red, .2f);
					GizmosUtility.DrawPhantom(door.gameObject, closedTransform);
				}
				if(openedTransform != null) {
					GizmosUtility.SetColor(Color.green, .2f);
					GizmosUtility.DrawPhantom(door.gameObject, openedTransform);
				}
			}
		}

		protected void OnValidate() {
			if(openedTransform.IsChildOf(door.transform))
				openedTransform = null;
			if(closedTransform.IsChildOf(door.transform))
				closedTransform = null;
			if(isOpened) {
				if(openedTransform) {
					door.transform.position = openedTransform.position;
					door.transform.rotation = openedTransform.rotation;
				}
			}
			else {
				if(closedTransform) {
					door.transform.position = closedTransform.position;
					door.transform.rotation = closedTransform.rotation;
				}
			}
		}
		#endregion
	}
}
#endif