#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore.Bordure {
	public partial class Detachable : MonoBehaviour {
		#region Life cycle
		protected void OnDrawGizmos() {
			GizmosUtility.SetColor(Color.yellow);
			Vector3 origin = transform.localToWorldMatrix.MultiplyPoint(ejectionOrigin);
			Vector3 velocity = transform.localToWorldMatrix.MultiplyVector(ejectionVelocity);
			Gizmos.DrawLine(origin, origin + velocity);
		}
		#endregion
	}
}
#endif