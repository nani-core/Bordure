using UnityEngine;
using System.Collections.Generic;
using System;

namespace NaniCore.UnityPlayground {
	/*
	 * Here implements how a portal controls its passengers -- that is,
	 * the objects that are passing through it.
	 */
	[RequireComponent(typeof(Collider))]
	public partial class Portal : MonoBehaviour {
		#region Passenger definitions
		public abstract class Passenger : IDisposable {
			public Portal portal;
			public readonly Transform root;
			public readonly Transform transform;
			public Mesh originalMesh, clampedMesh;
			protected Matrix4x4 lastRelativeMatrix;

			public Passenger(Portal portal, Transform root, Transform transform, Mesh originalMesh) {
				this.portal = portal;
				this.root = root;
				this.transform = transform;
				this.originalMesh = originalMesh;
				clampedMesh = Instantiate(originalMesh);
				clampedMesh.name = $"{originalMesh.name} (portal- clampped instance)";
			}

			public void ClampMeshToPortal() {
				if(originalMesh == null)
					return;

				Vector3[] clampedVertices = originalMesh.vertices;

				Matrix4x4 localToPortal = portal.transform.worldToLocalMatrix * transform.localToWorldMatrix;
				Matrix4x4 portalToLocal = localToPortal.inverse;

				for(int i = 0; i < clampedVertices.Length; ++i) {
					Vector3 positionUnderPortal = localToPortal.MultiplyPoint(clampedVertices[i]);
					if(positionUnderPortal.z > 0) {
						positionUnderPortal.z = 0;
						Vector3 restoredLocalPosition = portalToLocal.MultiplyPoint(positionUnderPortal);
						restoredLocalPosition.x = 0;
						restoredLocalPosition.y = 0;
						clampedVertices[i] = restoredLocalPosition;
					}
				}

				clampedMesh.vertices = clampedVertices;

				lastRelativeMatrix = localToPortal;
			}

			public abstract void Dispose();

			public bool ReclampMeshIfNecessary(Portal portal) {
				Matrix4x4 relativeMatrix = portal.transform.worldToLocalMatrix * transform.localToWorldMatrix;
				if(lastRelativeMatrix == relativeMatrix)
					return false;
				ClampMeshToPortal();
				return true;
			}
		}

		public class MeshFilterPassenger : Passenger {
			public readonly MeshFilter meshFilter;

			public MeshFilterPassenger(Portal portal, Transform root, MeshFilter meshFilter)
				: base(portal, root, meshFilter.transform, meshFilter.sharedMesh) {
				this.meshFilter = meshFilter;
				meshFilter.sharedMesh = clampedMesh;
			}

			public override void Dispose() {
				if(meshFilter != null)
					meshFilter.sharedMesh = originalMesh;
			}
		}

		public class MeshColliderPassenger : Passenger {
			public readonly MeshCollider meshCollider;

			public MeshColliderPassenger(Portal portal, Transform root, MeshCollider meshCollider)
				: base(portal, root, meshCollider.transform, meshCollider.sharedMesh) {
				this.meshCollider = meshCollider;
				meshCollider.sharedMesh = clampedMesh;
			}

			public override void Dispose() {
				if(meshCollider != null)
					meshCollider.sharedMesh = originalMesh;
			}
		}
		#endregion

		#region Fields
		protected Dictionary<Transform, HashSet<Passenger>> rootedPassengers = new Dictionary<Transform, HashSet<Passenger>>();
		#endregion

		#region Functions
		protected HashSet<Passenger> TryAddRoot(Transform root, out string error) {
			error = null;

			// Check if collider is already recorded as root.
			if(rootedPassengers.ContainsKey(root)) {
				error = "Root already recorded.";
				return rootedPassengers[root];
			}

			// Check if collider is a child of any recorded root.
			foreach(Transform existingRoot in rootedPassengers.Keys) {
				if(root.transform.IsChildOf(existingRoot.transform)) {
					error = $"Is child of an existing root {existingRoot.name}";
					return rootedPassengers[existingRoot];
				}
			}

			// Admit the collider as a new root and create a set for it.
			HashSet<Passenger> set = new HashSet<Passenger>();
			rootedPassengers.Add(root, set);
			return set;
		}

		protected void TryRemoveRoot(Transform root) {
			if(!rootedPassengers.ContainsKey(root))
				return;

			// Restore all the root's children passengers' mesh.
			foreach(Passenger passenger in rootedPassengers[root])
				passenger.Dispose();

			// Remove the record to the root itself.
			rootedPassengers.Remove(root);
		}

		protected void Teleport(Transform root) {
			if(root == null || twin == null)
				return;

			HashSet<Passenger> oldPassengerSet = null;

			if(rootedPassengers.ContainsKey(root)) {
				oldPassengerSet = rootedPassengers[root];
				rootedPassengers.Remove(root);
			}

			// TODO

			HashSet<Passenger> newPassengerSet = twin.TryAddRoot(root, out string error);
			if(oldPassengerSet != null) {
				foreach(Passenger passenger in oldPassengerSet) {
					newPassengerSet.Add(passenger);
					passenger.portal = twin;
					passenger.ClampMeshToPortal();
				}
			}
		}
		#endregion

		#region Life cycle
		protected void OnColliderEnterPortal(Collider collider) {
			Transform root = collider.transform;
			HashSet<Passenger> set = TryAddRoot(root, out string addError);

			if(string.IsNullOrEmpty(addError))
				Debug.Log($"{root.name} enters portal {name}.", this);

			foreach(MeshFilter meshFilter in root.GetComponentsInChildren<MeshFilter>(true))
				set.Add(new MeshFilterPassenger(this, root, meshFilter));

			foreach(MeshCollider meshCollider in root.GetComponentsInChildren<MeshCollider>(true))
				set.Add(new MeshColliderPassenger(this, root, meshCollider));
		}

		protected void OnColliderExitPortal(Collider collider) {
			Transform root = collider.transform;
			Debug.Log($"{root.name} exits portal {name}.", this);
			TryRemoveRoot(root.transform);
		}

		protected void LastUpdatePassenger() {
			// For every tracked passenger, clamp its mesh if it has moved
			// relatively to portal.
			foreach(HashSet<Passenger> set in rootedPassengers.Values) {
				foreach(Passenger passenger in set) {
					passenger.ReclampMeshIfNecessary(this);
				}
			}
		}
		#endregion
	}
}