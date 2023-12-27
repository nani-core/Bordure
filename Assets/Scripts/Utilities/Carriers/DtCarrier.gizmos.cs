#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore {
	public partial class DtCarrier : Carrier {
		#region Life cycle
		protected void OnDrawGizmos() {
			if(target != null) {
				if(closedTransform != null) {
					GizmosUtility.SetColor(Color.red, .2f);
					GizmosUtility.DrawPhantom(target.gameObject, closedTransform);
				}
				if(openedTransform != null) {
					GizmosUtility.SetColor(Color.green, .2f);
					GizmosUtility.DrawPhantom(target.gameObject, openedTransform);
				}
			}
		}

		protected void OnValidate() {
			if(openedTransform.IsChildOf(target.transform))
				openedTransform = null;
			if(closedTransform.IsChildOf(target.transform))
				closedTransform = null;
			if(isOpened) {
				if(openedTransform) {
					target.transform.position = openedTransform.position;
					target.transform.rotation = openedTransform.rotation;
				}
			}
			else {
				if(closedTransform) {
					target.transform.position = closedTransform.position;
					target.transform.rotation = closedTransform.rotation;
				}
			}
		}
		#endregion
	}
}
#endif