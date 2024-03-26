#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore.Bordure {
	public partial class Water : MonoBehaviour {
		#region Life cycle
		protected void OnValidate() {
			if(Application.isPlaying)
				return;

			Height = Height;

			if(transform.lossyScale.y != 1.0f) {
				Debug.LogWarning("The Y-scale of a water body must be unaltered (1.0). Please check the transform.", this);
			}
		}
		#endregion
	}
}
#endif