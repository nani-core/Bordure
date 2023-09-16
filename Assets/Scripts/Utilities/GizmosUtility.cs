#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore {
	public static class GizmosUtility {
		public static void DrawPhantom(this GameObject reference, Vector3 position, Quaternion rotation) {
			var baseTransform = reference.transform;
			var basis = Matrix4x4.TRS(baseTransform.position, baseTransform.rotation, Vector3.one);
			foreach(var meshFilter in reference.GetComponentsInChildren<MeshFilter>()) {
				var m = MathUtility.RelativeTransform(meshFilter.transform.localToWorldMatrix, basis);
				m = Matrix4x4.TRS(position, rotation, Vector3.one) * m;
				Gizmos.DrawMesh(meshFilter.sharedMesh, m.GetPosition(), m.rotation, m.lossyScale);
			}
		}
	}
}
#endif