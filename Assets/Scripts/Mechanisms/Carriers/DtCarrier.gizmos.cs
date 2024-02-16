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
			if(!(Target && openedTransform && closedTransform))
				return;

			if(openedTransform.IsChildOf(Target))
				openedTransform = null;
			if(closedTransform.IsChildOf(Target))
				closedTransform = null;
			if(isOpened) {
				if(openedTransform)
					Target.AlignWith(openedTransform);
			}
			else {
				if(closedTransform)
					Target.AlignWith(closedTransform);
			}
		}
		#endregion
	}
}
#endif