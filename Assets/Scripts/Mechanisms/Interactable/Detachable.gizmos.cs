#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore.Loopool {
	public partial class Detachable : Interactable {
		#region Life cycle
		protected void OnDrawGizmos() {
			if(useDetachingEjection) {
				GizmosUtility.SetColor(Color.yellow);
				Vector3 origin = transform.localToWorldMatrix.MultiplyPoint(ejectionOrigin);
				Vector3 velocity = transform.localToWorldMatrix.MultiplyVector(ejectionVelocity);
				Gizmos.DrawLine(origin, origin + velocity);
			}
		}
		#endregion
	}
}
#endif