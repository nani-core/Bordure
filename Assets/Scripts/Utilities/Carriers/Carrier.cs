using UnityEngine;

namespace NaniCore {
	public abstract class Carrier : MonoBehaviour {
		#region Serialized fields
		[SerializeField] protected Transform target;
		#endregion

		#region Interfaces
		public Transform Target {
			get => target;
			set => target = value;
		}
		#endregion
	}
}