using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Loopool {
	public partial class OpticalLoopShape : LoopShape {
		const float standardHeight = 216f;

		#region Serialized fields
		[Header("Optical")]
		[SerializeField] protected GameObject blasto;
		[SerializeField] protected GameObject gastro;
		[SerializeField][Range(0, 1)] private float thickness;
		[SerializeField][Range(0, 1)] private float thicknessTolerance;
#if UNITY_EDITOR
		[SerializeField] private bool showDebugLayer = false;
		[SerializeField][ShowIf("showDebugLayer")][Range(0, 1)] private float debugLayerOpacity = 1f;
#endif
		#endregion

		#region Fields
		private bool validated = false;
		private bool visible = false;
		private IEnumerable<Renderer> childRenderers;
		private float tolerance = 2f;
		#endregion

		#region Functions
		public override bool Validate(Transform eye) => validated;

		protected override void OnLoopShapeOpen() {
			base.OnLoopShapeOpen();
			validated = false;
		}

		private bool ValidateByMask(RenderTexture gastroMask, RenderTexture wholeMask) {
			wholeMask.InfectByValue(Color.clear, standardHeight * thickness, tolerance);
			var intersect = wholeMask.Duplicate();
			intersect.Intersect(gastroMask);
			bool hasIntersection = intersect.HasValue(Color.white, 4, tolerance);
			intersect.Destroy();
			if(!hasIntersection)
				return false;

			var validationMask = wholeMask.Duplicate();
			validationMask.Difference(gastroMask);
			validationMask.InfectByValue(Color.clear, standardHeight * thicknessTolerance);
			var perfectlyMatched = !validationMask.HasValue(Color.white, 4, tolerance);
			validationMask.Destroy();

			return perfectlyMatched;
		}

		private bool PerformValidation() {
			if(!visible || blasto == null || gastro == null)
				return false;

			var mrtTexture = RenderTexture.GetTemporary(Mathf.FloorToInt(standardHeight * Screen.width / Screen.height), Mathf.FloorToInt(standardHeight));
			mrtTexture.SetValue(Color.clear);

			// Render masks
			Color blastoColor = Color.red, gastroColor = Color.green;
			{
				var maskTexture = RenderUtility.CreateScreenSizedRT();

				maskTexture.SetValue(Color.clear);
				maskTexture.RenderMask(blasto, Camera.main);
				maskTexture.ReplaceValueByValue(Color.white, blastoColor);
				mrtTexture.Overlay(maskTexture);

				maskTexture.SetValue(Color.clear);
				maskTexture.RenderMask(gastro, Camera.main);
				maskTexture.ReplaceValueByValue(Color.white, gastroColor);
				mrtTexture.Overlay(maskTexture);

				maskTexture.Destroy();
			}

			if(!mrtTexture.HasValue(gastroColor)) {
				mrtTexture.ReplaceValueByValue(blastoColor, Color.red, tolerance);
				mrtTexture.ReplaceTextureByValue(Color.clear, GameManager.Instance.WorldView, tolerance);
				// Don't forget to release temporary RT on early returns!
				mrtTexture.Destroy();
				return false;
			}

			var gastroMask = mrtTexture.Duplicate();
			gastroMask.IndicateByValue(gastroColor, tolerance);

			var wholeMask = mrtTexture.Duplicate();
			wholeMask.ReplaceValueByValue(blastoColor, Color.white, tolerance);
			wholeMask.ReplaceValueByValue(gastroColor, Color.white, tolerance);
			wholeMask.IndicateByValue(Color.white, tolerance);

			bool validated = ValidateByMask(gastroMask, wholeMask);

			wholeMask.ReplaceValueByValue(Color.white, validated ? Color.green : Color.red, tolerance);
			wholeMask.filterMode = FilterMode.Point;

			if(showDebugLayer) {
				var resample = mrtTexture.Resample(new Vector2(Screen.width, Screen.height).Floor());
				resample.Overlay(GameManager.Instance.WorldView, debugLayerOpacity);
				Graphics.Blit(resample, null as RenderTexture);
				resample.Destroy();
			}
			mrtTexture.Destroy();

			wholeMask.Destroy();
			gastroMask.Destroy();

			return validated;
		}

		public void DestroyGastro() {
			if(gastro == null)
				return;
			gastro.gameObject.SetActive(false);
			gastro = null;
		}

		public void Stamp() {
			StampHandler.Stamp(blasto, Camera.main);
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			base.Start();

			childRenderers = GetComponentsInChildren<Renderer>();
		}

		protected void Update() {
			visible = childRenderers.Any(r => r.isVisible);
			if(GameManager.Instance?.Protagonist != null) {
				validated = PerformValidation();
			}
		}
		#endregion
	}
}