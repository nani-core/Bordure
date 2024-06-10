using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NaniCore.Bordure.OpticalTest {
	public class OpticalTestManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Camera mainCamera;
		[SerializeField] private MaskableGraphic display;
		#endregion

		#region Fields
		private RenderTexture renderRt;
		private RenderTexture displayRt;
		#endregion

		#region Life cycle
		protected void Start() {
			renderRt = RenderUtility.CreateScreenSizedRT();
			mainCamera.targetTexture = renderRt;
			
			displayRt = RenderUtility.CreateScreenSizedRT();
			display.material.mainTexture = displayRt;

			RenderPipelineManager.endFrameRendering += OnRPMEndFrameRendering;
		}

		protected void OnDestroy() {
			RenderPipelineManager.endFrameRendering -= OnRPMEndFrameRendering;

			displayRt.Destroy();

			renderRt.Destroy();
		}

		protected void OnMainCameraFinishedRendering() {
			displayRt.Clear();
			Graphics.Blit(renderRt, displayRt);
		}
		#endregion

		#region Functions
		private void OnRPMEndFrameRendering(ScriptableRenderContext context, Camera[] cameras) {
			OnMainCameraFinishedRendering();
		}
		#endregion
	}
}