using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.UnityPlayground {
	/*
	 * Here implements how a portal controls objects passing through it.
	 */
	[RequireComponent(typeof(Collider))]
	public partial class Portal : MonoBehaviour {
		#region Functions
		protected Matrix4x4 CalculateRelativeTransformFromSelf(Transform other) {
			return other.worldToLocalMatrix * transform.localToWorldMatrix;
		}

		protected void OnObjectEnterPortal(Collider collider) {
			// TODO
			Debug.Log($"{collider.name} enters portal {name}.", this);
		}

		protected void OnObjectExitPortal(Collider collider) {
			// TODO
			Debug.Log($"{collider.name} exits portal {name}.", this);
		}
		#endregion
	}
}