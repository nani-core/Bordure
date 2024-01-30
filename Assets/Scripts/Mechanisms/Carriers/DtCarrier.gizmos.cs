#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore {
	public partial class DtCarrier : Carrier {
		#region Life cycle
		protected void OnDrawGizmos() {
			if(Target != null) {
				if(closedTransform != null) {
					GizmosUtility.SetColor(Color.red, .2f);
					GizmosUtility.DrawPhantom(Target.gameObject, closedTransform);
				}
				if(openedTransform != null) {
					GizmosUtility.SetColor(Color.green, .2f);
					GizmosUtility.DrawPhantom(Target.gameObject, openedTransform);
				}
			}
		}

		protected void OnValidate() {
			if(openedTransform.IsChildOf(Target.transform))
				openedTransform = null;
			if(closedTransform.IsChildOf(Target.transform))
				closedTransform = null;
			if(isOpened) {
				if(openedTransform) {
					Target.transform.position = openedTransform.position;
					Target.transform.rotation = openedTransform.rotation;
				}
			}
			else {
				if(closedTransform) {
					Target.transform.position = closedTransform.position;
					Target.transform.rotation = closedTransform.rotation;
				}
			}
		}
		#endregion
	}
}
#endif