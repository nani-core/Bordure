using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NaniCore.Bordure {
	public class OpticalValidator : LoopshapeValidator {
		#region Constants
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
		private Coroutine validatingCoroutine;
		#endregion

		#region Functions
		protected static GameSettings GameSettings => GameManager.Instance?.Settings;

		protected override bool Validate() => isActiveAndEnabled && validated;

		private IEnumerator PerformValidationCoroutine(System.Action<bool> continuation) {
			if(!visible || gastro == null || !isActiveAndEnabled || !gastro.activeInHierarchy) {
				continuation(false);
				yield break;
			}

			// Rough, geometry-based invalidation.

			if(gastro != null) {
				// Invalidate if not focusing on the gastro or the blasto (self).
				var lookingAtObject = GameManager.Instance?.Protagonist?.LookingAtObject;
				if(lookingAtObject == null || !(lookingAtObject.IsChildOf(gastro) || lookingAtObject.IsChildOf(gameObject))) {
					continuation(false);
					yield break;
				}
			}

			// Real optical validation.
			Vector2Int validationSize = (new Vector2(Screen.width / Screen.height, 1) * GameSettings.standardHeight).Floor();

			// Render masks
			Camera mainCamera = GameManager.Instance?.MainCamera;
			if(mainCamera == null) {
				continuation(false);
				yield break;
			}
			RenderTexture
				blastoMask = RenderUtility.CreateRT(validationSize),
				gastroMask = RenderUtility.CreateRT(validationSize);
			yield return WaitForMaskRenderingOpportunity();
			blastoMask.RenderMask(gameObject, mainCamera);
			blastoMask.DenoiseMask();
			yield return WaitForMaskRenderingOpportunity();
			gastroMask.RenderMask(gastro, mainCamera);
			gastroMask.InfectByValue(Color.white, 2);
			yield return WaitForMaskRenderingOpportunity();

			float bordureWidth;
			{
				Vector2Int loopshapeBound = Vector2.Scale(ViewportBoundOfGameObject(gameObject).size, blastoMask.Size()).Ceil();
				int loopshapeSize = Mathf.Min(loopshapeBound.x, loopshapeBound.y);
				loopshapeSize = Mathf.Min(loopshapeSize, GameSettings.standardHeight);
				bordureWidth = loopshapeSize * GameSettings.bordureThicknessRatio;
			}
			bool validated = ValidateByMask(blastoMask, gastroMask, bordureWidth);

			blastoMask.Destroy();
			gastroMask.Destroy();

			continuation(validated);
		}

		private static CustomYieldInstruction WaitForMaskRenderingOpportunity() {
			float start = Time.time;
			return new WaitUntil(() => {
				float timePassed = Time.time - start;
				return timePassed >= GameSettings.desiredOpticalValidationInterval;
			});
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
			validationMask.InfectByValue(Color.clear, bordureWidth * GameSettings.bordureThicknessTolerance);

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

				GameManager.Instance.DrawDebugOverlayFrame(representative, debugLayerOpacity);
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

		private void FinishValidation(bool result) {
			if(result != validated) {
				if(result)
					Debug.Log($"{Loopshape.name} is validated.", Loopshape);
				else
					Debug.Log($"{Loopshape.name} is invalidated.", Loopshape);
			}
			validated = result;
			validatingCoroutine = null;
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
			if(validatingCoroutine == null) {
				validatingCoroutine = StartCoroutine(PerformValidationCoroutine(FinishValidation));
			}
		}
		#endregion
	}
}