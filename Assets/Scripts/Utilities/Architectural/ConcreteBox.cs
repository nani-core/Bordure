using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;

namespace NaniCore {
	public class ConcreteBox : ArchitectureGenerator {
		[System.Serializable]
		public struct FaceConfig {
			public GameObject[] tiles;
			public bool dontGenerateConcrete;
			public Material concreteMaterial;
			[Min(0)] public float concreteThickness;
			[Min(0)] public float concreteDepth;
			public List<GameObject> hollowObjects;
			[ShowIf("inward")] public bool preventOverlapping;
			[ShowIf("inward")] public bool forceFillCorner;
		}

		#region Serialized fields
		public Vector3Int count = Vector3Int.one * 8;
		public Vector3 spacing = Vector3.one;
		public bool inward = false;

		[Label("Face Config")] public FaceConfig defaultConfig;

		[Label("X+ (right)")] public bool xp = true;
		[Label("Override")][ShowIf("xp")] public bool xpOverride = false;
		[Label("")][ShowIf(EConditionOperator.And, "xpOverride", "xp")] public FaceConfig xpConfig;

		[Label("X- (left)")] public bool xm = false;
		[Label("Override")][ShowIf("xm")] public bool xmOverride = false;
		[Label("")][ShowIf(EConditionOperator.And, "xmOverride", "xm")] public FaceConfig xmConfig;

		[Label("Y+ (up)")] public bool yp = true;
		[Label("Override")][ShowIf("yp")] public bool ypOverride = false;
		[Label("")][ShowIf(EConditionOperator.And, "ypOverride", "yp")] public FaceConfig ypConfig;

		[Label("Y- (down)")] public bool ym = false;
		[Label("Override")][ShowIf("ym")] public bool ymOverride = false;
		[Label("")][ShowIf(EConditionOperator.And, "ymOverride", "ym")] public FaceConfig ymConfig;

		[Label("Z+ (forward)")] public bool zp = true;
		[Label("Override")][ShowIf("zp")] public bool zpOverride = false;
		[Label("")][ShowIf(EConditionOperator.And, "zpOverride", "zp")] public FaceConfig zpConfig;

		[Label("Z- (backward)")] public bool zm = false;
		[Label("Override")][ShowIf("zm")] public bool zmOverride = false;
		[Label("")][ShowIf(EConditionOperator.And, "zmOverride", "zm")] public FaceConfig zmConfig;
		#endregion

		struct FaceInfo {
			public string name;
			public int dim;
			public (int, int) countDim;
			public Vector3 euler;
			public Vector3 offset;
			public FaceConfig config;
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
					config = xpOverride ? xpConfig : defaultConfig,
				};
				if(xm) yield return new FaceInfo {
					name = "X-",
					dim = 0,
					countDim = (2, 1),
					euler = Vector3.up * 90,
					offset = Vector3.left,
					config = xmOverride ? xmConfig : defaultConfig,
				};
				if(yp) yield return new FaceInfo {
					name = "Y+",
					dim = 1,
					countDim = (0, 2),
					euler = Vector3.right * 90,
					offset = Vector3.up,
					config = ypOverride ? ypConfig : defaultConfig,
				};
				if(ym) yield return new FaceInfo {
					name = "Y-",
					dim = 1,
					countDim = (0, 2),
					euler = Vector3.right * -90,
					offset = Vector3.down,
					config = ymOverride ? ymConfig : defaultConfig,
				};
				if(zp) yield return new FaceInfo {
					name = "Z+",
					dim = 2,
					countDim = (0, 1),
					euler = Vector3.up * 180,
					offset = Vector3.forward,
					config = zpOverride ? zpConfig : defaultConfig,
				};
				if(zm) yield return new FaceInfo {
					name = "Z-",
					dim = 2,
					countDim = (0, 1),
					euler = Vector3.zero,
					offset = Vector3.back,
					config = zmOverride ? zmConfig : defaultConfig,
				};
			}
		}

		protected override void Construct(Transform under, Instantiator instantiator) {
			bool shouldGenerateConcrete = true;
#if DEBUG && UNITY_EDITOR
			if(!Application.isPlaying) {
				var manager = FindObjectOfType<Bordure.GameManager>(true);
				if(manager?.Settings != null) {
					shouldGenerateConcrete = manager.Settings.generateConcreteInEditMode;
				}
			}
#endif

			foreach(var face in Faces) {
				GameObject faceObj = new($"{gameObject.name} (wall {face.name})");
				faceObj.isStatic = gameObject.isStatic;

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
				var meshTile = faceObj.AddComponent<MeshTile>();
				{
					meshTile.tiles = face.config.tiles?.ToArray() ?? new GameObject[0];
					meshTile.seed = (int)HashUtility.Hash(seed, face.name.GetHashCode());

					meshTile.i = Vector3.right * spacing.x;
					meshTile.j = Vector3.up * spacing.y;
					meshTile.k = Vector3.forward * spacing.z;

					Vector3Int count = Vector3Int.one;
					count[0] = this.count[face.countDim.Item1];
					count[1] = this.count[face.countDim.Item2];
					meshTile.count = count;

					meshTile.uvw = Vector3.one * .5f;

					meshTile.hollowObjects = face.config.hollowObjects;
				}
				meshTile.Construct();

				// Generate concrete.
				if(!face.config.dontGenerateConcrete) {
					if(shouldGenerateConcrete) {
						var concreteObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
						concreteObj.name = $"{gameObject.name} (concrete {face.name})";
						concreteObj.isStatic = gameObject.isStatic;
						concreteObj.layer = LayerMask.NameToLayer("Concrete");
						var concreteTransform = concreteObj.transform;
						concreteTransform.SetParent(faceTransform, false);

						concreteTransform.localPosition = Vector3.forward * (face.config.concreteThickness * .5f + face.config.concreteDepth);

						var size = Vector3.one * face.config.concreteThickness;
						var hSize = Vector3.Scale(count, spacing);
						hSize += Vector3.one * ((inward && !face.config.preventOverlapping ? 1 : -1) * face.config.concreteDepth * 2);
						if(inward && face.config.forceFillCorner)
							hSize += spacing * 2;
						size[0] = hSize[face.countDim.Item1];
						size[1] = hSize[face.countDim.Item2];
						concreteTransform.localScale = size;

						var renderer = concreteObj.GetComponent<Renderer>();
						if(face.config.concreteMaterial != null)
							renderer.sharedMaterial = face.config.concreteMaterial;
						else
							renderer.sharedMaterials = new Material[0];
					}
				}
			}
		}
		#endregion
	}
}