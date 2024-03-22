using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NaniCore.Stencil {
	public class OpticalValidator : LoopshapeValidator {
		#region Constants
		/// <summary>
		/// The standard height of the RT on which the validation algorithm will be performed.
		/// </summary>
		/// <remarks>
		/// The higher this value is, the more accurate the validation algorithm's result will be;
		/// at the meantime the performance cost will be higher.
		/// </remarks>
		const int standardHeight = 216;
		// The standard thickness of the bordure, measured in portion.
		const float thickness = 1.0f / 6.0f;
		/// <summary>
		/// In the last step of the validation algorithm, how much remaining areas are allowed.
		/// </summary>
		/// <remarks>
		/// Measured in the portion of the bordure width.
		/// </remarks>
		const float thicknessTolerance = 0.3f;
		// Debug colors.
		private readonly
			Color blastoColor = Color.red,
			gastroColor = Color.green,
			bordureColor = Color.yellow,
			validationColor = Color.blue;
		#endregion

		#region Serialized fields
		[SerializeField] public GameObject gastro;
#if DEBUG
		[SerializeField] private bool showDebugLayer = false;
		[SerializeField][ShowIf("showDebugLayer")][Range(0, 1)] private float debugLayerOpacity = .5f;
#endif
		#endregion

		#region Fields
		private bool validated = false;
		private bool visible = false;
		private IEnumerable<Renderer> childRenderers;
		#endregion

		#region Functions
		protected override bool Validate() => isActiveAndEnabled && validated;

		private bool PerformValidation() {
			if(!visible || gastro == null || !isActiveAndEnabled || !gastro.activeInHierarchy)
				return false;

			// Rough, geometry-based invalidation.

			if(gastro != null) {
				// Invalidate if not focusing on the gastro or the blasto (self).
				var lookingAtObject = GameManager.Instance?.Protagonist?.LookingAtObject;
				if(lookingAtObject == null || !(lookingAtObject.IsChildOf(gastro) || lookingAtObject.IsChildOf(gameObject)))
					return false;
			}

			// Real optical validation.
			Vector2Int validationSize = (new Vector2(Screen.width / Screen.height, 1) * standardHeight).Floor();

			// Render masks
			RenderTexture
				blastoMask = RenderUtility.CreateRT(validationSize),
				gastroMask = RenderUtility.CreateRT(validationSize);
			blastoMask.RenderMask(gameObject, GameManager.Instance?.MainCamera);
			gastroMask.RenderMask(gastro, GameManager.Instance?.MainCamera);

			float bordureWidth;
			{
				Vector2Int loopshapeBound = Vector2.Scale(ViewportBoundOfGameObject(gameObject).size, blastoMask.Size()).Ceil();
				int loopshapeSize = Mathf.Min(loopshapeBound.x, loopshapeBound.y);
				loopshapeSize = Mathf.Min(loopshapeSize, standardHeight);
				bordureWidth = loopshapeSize * thickness;
			}
			bool validated = ValidateByMask(blastoMask, gastroMask, bordureWidth);

			blastoMask.Destroy();
			gastroMask.Destroy();

			return validated;
		}

		private bool ValidateByMask(RenderTexture blastoMask, RenderTexture gastroMask, float bordureWidth) {
			if(!blastoMask.HasValue(Color.white) || !gastroMask.HasValue(Color.white)) {
				// Invalidate if the blasto or the gastro is not visible.
				return false;
			}

			RenderTexture unionMask, bordureMask, validationMask;
			unionMask = blastoMask.Duplicate();
			unionMask.Overlay(gastroMask);

			bordureMask = unionMask.Duplicate();
			bordureMask.InfectByValue(Color.clear, bordureWidth);
			bordureMask.Difference(unionMask);

			validationMask = unionMask.Duplicate();
			// Shrink the union by the bordure width.
			validationMask.InfectByValue(Color.clear, bordureWidth);
			// Take the difference with the gastro mask.
			// They should be of greate similarity.
			// After this step, there should be only broken slices that are not clear.
			validationMask.Difference(gastroMask);
			// One last shrink.
			// If the loopshape is good, there shall be no more non-clear pixels.
			validationMask.InfectByValue(Color.clear, bordureWidth * thicknessTolerance);

			var validation = !validationMask.HasValue(Color.white);

#if DEBUG
			if(showDebugLayer) {
				blastoMask.ReplaceValueByValue(Color.white, blastoColor);
				bordureMask.ReplaceValueByValue(Color.white, bordureColor);
				gastroMask.ReplaceValueByValue(Color.white, gastroColor);
				validationMask.ReplaceValueByValue(Color.white, validationColor);

				var representative = blastoMask;
				representative.Overlay(bordureMask);
				representative.Overlay(gastroMask);
				representative.Overlay(validationMask);

				GameManager.Instance.DrawDebugFrame(representative, debugLayerOpacity);
			}
#endif

			validationMask.Destroy();
			unionMask.Destroy();
			bordureMask.Destroy();

			return validation;
		}

		private static Rect ViewportBoundOfGameObject(GameObject go) {
			var camera = GameManager.Instance?.MainCamera;
			if(camera == null)
				return default;

			List<Vector3> boundVertices = new();
			foreach(var renderer in go.GetComponentsInChildren<Renderer>()) {
				var bounds = renderer.bounds;
				boundVertices.Add(bounds.min);
				boundVertices.Add(bounds.max);
			}
			var screenPoints = boundVertices.Select(v => {
				Vector2 sp = camera.WorldToViewportPoint(v);
				sp.y = 1 - sp.y;
				return sp;
			});
			return MathUtility.MakeRect(screenPoints.ToArray());
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			base.Start();
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