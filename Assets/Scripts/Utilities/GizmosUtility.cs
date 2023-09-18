#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore {
	public static class GizmosUtility {
		public static void SetColor(Color color, float alpha = 1) {
			color.a = alpha;
			Gizmos.color = color;
		}

		public static void DrawPhantom(GameObject reference, Vector3 position, Quaternion rotation) {
			var baseTransform = reference.transform;
			var basis = Matrix4x4.TRS(baseTransform.position, baseTransform.rotation, Vector3.one);
			foreach(var meshFilter in reference.GetComponentsInChildren<MeshFilter>()) {
				var m = MathUtility.RelativeTransform(meshFilter.transform.localToWorldMatrix, basis);
				m = Matrix4x4.TRS(position, rotation, Vector3.one) * m;
				Gizmos.DrawMesh(meshFilter.sharedMesh, m.GetPosition(), m.rotation, m.lossyScale);
			}
		}

		public static void DrawPhantom(GameObject reference, Transform transform)
			=> DrawPhantom(reference, transform.position, transform.rotation);

		public static void DrawPolygon(IEnumerable<Vector3> vertices) {
			if(vertices == null)
				return;
			int length = vertices.Count();
			if(length <= 1)
				return;
			var list = vertices.ToList();
			for(int i = 0; i < length; ++i)
				Gizmos.DrawLine(list[i], list[(i + 1) % length]);
		}
	}
}
#endif