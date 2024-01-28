#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore.Stencil {
	public partial class Water : MonoBehaviour {
		#region Life cycle
		protected void OnValidate() {
			if(Application.isPlaying)
				return;
			Height = Height;
		}
		#endregion
	}
}
#endif