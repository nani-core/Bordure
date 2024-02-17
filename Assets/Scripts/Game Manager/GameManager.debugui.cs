using UnityEngine;

namespace NaniCore.Stencil {
	public partial class GameManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private UnityEngine.UI.RawImage debugLayer;
		#endregion

		#region Fields
		private RenderTexture debugFrame;
		#endregion

		#region Interfaces
		public void DrawDebugFrame(Texture texture, float opacity = 1f) {
			if(debugFrame == null)
				return;
			debugFrame.Overlay(texture, opacity);
		}
		#endregion

		#region Life cycle
		protected void InitializeDebugUi() {
			if(debugLayer == null)
				return;

			debugLayer.enabled = true;
			debugLayer.texture = debugFrame = RenderUtility.CreateScreenSizedRT();
			debugFrame.SetValue(Color.clear);
		}

		protected void UpdateDebugUi() {
			debugFrame?.SetValue(Color.clear);
		}

		protected void FinalizeDebugUi() {
			debugFrame?.Destroy();
		}
		#endregion
	}
}