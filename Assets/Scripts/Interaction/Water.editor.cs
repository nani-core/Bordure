#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore.UnityPlayground {
	public partial class Water : MonoBehaviour {
		#region Life cycle
		protected void OnValidate() {
			if(Application.isPlaying)
				return;
			Height = height;
		}
		#endregion
	}
}
#endif