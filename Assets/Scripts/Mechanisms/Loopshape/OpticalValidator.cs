using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;

namespace NaniCore.Stencil {
	public class OpticalValidator : LoopshapeValidator {
		const float standardHeight = 216f;

		#region Serialized fields
		[SerializeField] public GameObject gastro;
		[SerializeField][Range(0, 1)] private float thickness;
		[SerializeField][Range(0, 1)] private float thicknessTolerance;
		[SerializeField] private float validationTolerance = 2f;
#if DEBUG
		[SerializeField] private bool showDebugLayer = false;
		[SerializeField][ShowIf("showDebugLayer")][Range(0, 1)] private float debugLayerOpacity = 1f;
#endif
		#endregion

		#region Fields
		private bool validated = false;
		private bool visible = false;
		private IEnumerable<Renderer> childRenderers;
		#endregion

		#region Functions
		protected override bool Validate() => validated;

		private bool ValidateByMask(RenderTexture gastroMask, RenderTexture wholeMask) {
			wholeMask.InfectByValue(Color.clear, standardHeight * thickness, validationTolerance);
			var intersect = wholeMask.Duplicate();
			intersect.Intersect(gastroMask);
			bool hasIntersection = intersect.HasValue(Color.white, 4, validationTolerance);
			intersect.Destroy();
			if(!hasIntersection)
				return false;

			var validationMask = wholeMask.Duplicate();
			validationMask.Difference(gastroMask);
			validationMask.InfectByValue(Color.clear, standardHeight * thicknessTolerance);
			var perfectlyMatched = !validationMask.HasValue(Color.white, 4, validationTolerance);
			validationMask.Destroy();

			return perfectlyMatched;
		}

		private bool PerformValidation() {
			if(!visible || gastro == null)
				return false;

			// Rough, geometry-based invalidation.

			if(gastro != null) {
				// Invalidate if not focusing on the gastro.
				var protagonist = GameManager.Instance?.Protagonist;
				if(protagonist == null || !protagonist.IsLookingAt(gameObject))
					return false;
			}

			// Real optical validation.

			var mrtTexture = RenderTexture.GetTemporary(Mathf.FloorToInt(standardHeight * Screen.width / Screen.height), Mathf.FloorToInt(standardHeight));
			mrtTexture.SetValue(Color.clear);

			// Render masks
			Color blastoColor = Color.red, gastroColor = Color.green;
			{
				var maskTexture = RenderUtility.CreateScreenSizedRT();
				maskTexture.SetValue(Color.clear);

				maskTexture.RenderMask(gameObject, GameManager.Instance?.MainCamera);
				maskTexture.ReplaceValueByValue(Color.white, blastoColor);

				maskTexture.RenderMask(gastro, GameManager.Instance?.MainCamera);
				maskTexture.ReplaceValueByValue(Color.white, gastroColor);

				mrtTexture.Overlay(maskTexture);
				maskTexture.Destroy();
			}

#if DEBUG
			if(showDebugLayer)
				GameManager.Instance.DrawDebugFrame(mrtTexture, debugLayerOpacity);
#endif

			if(!mrtTexture.HasValue(gastroColor)) {
				// Don't forget to release temporary RT on early returns!
				mrtTexture.Destroy();
				return false;
			}

			var gastroMask = mrtTexture.Duplicate();
			gastroMask.IndicateByValue(gastroColor, validationTolerance);

			var wholeMask = mrtTexture.Duplicate();
			wholeMask.ReplaceValueByValue(blastoColor, Color.white, validationTolerance);
			wholeMask.ReplaceValueByValue(gastroColor, Color.white, validationTolerance);
			wholeMask.IndicateByValue(Color.white, validationTolerance);

			bool validated = ValidateByMask(gastroMask, wholeMask);

			mrtTexture.Destroy();

			wholeMask.Destroy();
			gastroMask.Destroy();

			return validated;
		}
		#endregion

		#region Life cycle
		protected void Start() {
			childRenderers = GetComponentsInChildren<Renderer>();
		}

		protected new void Update() {
			base.Update();

			visible = childRenderers.Any(r => r.isVisible);
			validated = PerformValidation();
		}
		#endregion
	}
}