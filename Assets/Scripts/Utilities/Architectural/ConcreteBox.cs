using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;

namespace NaniCore {
	public class ConcreteBox : ArchitectureGenerator {
		#region Serialized fields
		[Header("Material")]
		public GameObject tile;
		public Material concreteMaterial;

		[Header("Geometry")]
		public Vector3Int count = Vector3Int.one * 8;
		public Vector3 spacing = Vector3.one;
		[Min(0)] public float concreteThickness = 1;
		[Min(0)] public float concreteDepth = .01f;
		public List<GameObject> hollowObjects = new List<GameObject>();
		public bool inward = false;
		[ShowIf("inward")] public bool preventOverlapping = false;
		[ShowIf("inward")] public bool forceFillCorner = false;

		[Header("Generation")]
		[Label("X+ (right)")] public bool xp = true;
		[Label("X- (left)")] public bool xm = false;
		[Label("Y+ (up)")] public bool yp = true;
		[Label("Y- (down)")] public bool ym = false;
		[Label("Z+ (forward)")] public bool zp = true;
		[Label("Z- (backward)")] public bool zm = false;
		#endregion

		struct FaceInfo {
			public string name;
			public int dim;
			public (int, int) countDim;
			public Vector3 euler;
			public Vector3 offset;
		}

		#region Functions
		protected override string GizmozRootName => "$ConcreteBoxGizmosRoot";

		IEnumerable<FaceInfo> Faces {
			get {
				if(xp) yield return new FaceInfo {
					name = "X+",
					dim = 0,
					countDim = (2, 1),
					euler = Vector3.up * -90,
					offset = Vector3.right,
				};
				if(xm) yield return new FaceInfo {
					name = "X-",
					dim = 0,
					countDim = (2, 1),
					euler = Vector3.up * 90,
					offset = Vector3.left,
				};
				if(yp) yield return new FaceInfo {
					name = "Y+",
					dim = 1,
					countDim = (0, 2),
					euler = Vector3.right * 90,
					offset = Vector3.up,
				};
				if(ym) yield return new FaceInfo {
					name = "Y-",
					dim = 1,
					countDim = (0, 2),
					euler = Vector3.right * -90,
					offset = Vector3.down,
				};
				if(zp) yield return new FaceInfo {
					name = "Z+",
					dim = 2,
					countDim = (0, 1),
					euler = Vector3.up * 180,
					offset = Vector3.forward,
				};
				if(zm) yield return new FaceInfo {
					name = "Z-",
					dim = 2,
					countDim = (0, 1),
					euler = Vector3.zero,
					offset = Vector3.back,
				};
			}
		}

		protected override void Construct(Transform under, Instantiator instantiator) {
			foreach(var face in Faces) {
				var faceObj = new GameObject($"{gameObject.name} (wall {face.name})");

				var faceTransform = faceObj.transform;
				faceTransform.SetParent(under, false);
				faceTransform.localPosition = Vector3.Scale(Vector3.Scale(face.offset, spacing), (Vector3)count * .5f);
				var orientation = Quaternion.Euler(face.euler);
				if(inward) {
					orientation.ToAngleAxis(out var angle, out var axis);
					orientation = Quaternion.AngleAxis(angle + 180, axis);
				}
				faceTransform.localRotation = orientation;

				// Initialze and generate tile.
				var tile = faceObj.AddComponent<MeshTile>();
				{
					tile.tile = this.tile;

					tile.i = Vector3.right * spacing.x;
					tile.j = Vector3.up * spacing.y;
					tile.k = Vector3.forward * spacing.z;

					Vector3Int count = Vector3Int.one;
					count[0] = this.count[face.countDim.Item1];
					count[1] = this.count[face.countDim.Item2];
					tile.count = count;

					tile.uvw = Vector3.one * .5f;

					tile.hollowObjects = hollowObjects.ToList();
				}
				tile.Construct();

				// Generate concrete.
				{
					var concreteObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
					concreteObj.name = $"{gameObject.name} (concrete {face.name})";
					var concreteTransform = concreteObj.transform;
					concreteTransform.SetParent(faceTransform, false);

					concreteTransform.localPosition = Vector3.forward * (concreteThickness * .5f + concreteDepth);

					var size = Vector3.one * concreteThickness;
					var hSize = Vector3.Scale(count, spacing);
					hSize += Vector3.one * ((inward && !preventOverlapping ? 1 : -1) * concreteDepth * 2);
					if(inward && forceFillCorner)
						hSize += spacing * 2;
					size[0] = hSize[face.countDim.Item1];
					size[1] = hSize[face.countDim.Item2];
					concreteTransform.localScale = size;

					if(concreteMaterial != null)
						concreteObj.GetComponent<Renderer>().sharedMaterial = concreteMaterial;
				}
			}
		}
		#endregion
	}
}