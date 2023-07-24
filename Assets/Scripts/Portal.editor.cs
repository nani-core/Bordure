#if UNITY_EDITOR
using UnityEngine;

namespace NaniCore.UnityPlayground {
	public partial class Portal : MonoBehaviour {
		#region Fields
		private Portal previousTwin;
		#endregion

		#region Life cycle
		protected void OnValidate() {
			// Pair/unpair twin when changing.
			if(twin != previousTwin) {
				if(twin != null) {
					twin.twin = this;
				}
				else {
					if(previousTwin != null)
						previousTwin.twin = null;
				}
				previousTwin = twin;
			}
		}
		#endregion
	}
}
#endif