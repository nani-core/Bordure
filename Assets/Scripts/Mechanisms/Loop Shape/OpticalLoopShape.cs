using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	public class OpticalLoopShape : LoopShape {
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
		private Mrt blastoMrt, gastroMrt;
		private RenderTexture mrtTexture;
		private bool validated = false;
		private bool visible = false;
		private IEnumerable<Renderer> childRenderers;
		private float tolerance = 2f;
		#endregion

		#region Functions
		public override bool Validate(Transform eye) => validated;

		protected override void OnLoopShapeOpen() {
			validated = false;
			onOpen?.Invoke();
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

		private bool PerformValidation(Camera camera, RenderTexture cameraOutput) {
			if(!visible || blastoMrt == null || gastroMrt == null)
				return false;

			mrtTexture.SetValue(Color.clear);
			mrtTexture.Overlay(blastoMrt.MrtTexture);
			mrtTexture.Overlay(gastroMrt.MrtTexture);

			var downsampled = mrtTexture.Resample(((Vector2)mrtTexture.Size() * (standardHeight / mrtTexture.height)).Floor());

			if(!downsampled.HasValue(gastroMrt.mrtValue)) {
				mrtTexture.ReplaceValueByValue(blastoMrt.mrtValue, Color.red, tolerance);
				mrtTexture.ReplaceTextureByValue(Color.clear, cameraOutput, tolerance);
				// Don't forget to release temporary RT on early returns!
				downsampled.Destroy();
				return false;
			}

			var gastroMask = downsampled.Duplicate();
			gastroMask.IndicateByValue(gastroMrt.mrtValue, tolerance);

			var wholeMask = downsampled.Duplicate();
			wholeMask.ReplaceValueByValue(blastoMrt.mrtValue, Color.white, tolerance);
			wholeMask.ReplaceValueByValue(gastroMrt.mrtValue, Color.white, tolerance);
			wholeMask.IndicateByValue(Color.white, tolerance);

			downsampled.Destroy();

			bool validated = ValidateByMask(gastroMask, wholeMask);

			wholeMask.ReplaceValueByValue(Color.white, validated ? Color.green : Color.red, tolerance);
			wholeMask.filterMode = FilterMode.Point;

			if(showDebugLayer) {
				Graphics.Blit(wholeMask, mrtTexture);
				mrtTexture.ReplaceTextureByValue(Color.clear, cameraOutput, tolerance);
			}

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
			blastoMrt?.Stamp(MainCamera.Instance?.Camera);
		}
		#endregion

		#region Message handlers
		private void OnRendered(Camera camera, RenderTexture cameraOutput) {
			if(!visible)
				return;
			validated = PerformValidation(camera, cameraOutput);
			if(showDebugLayer)
				cameraOutput.Overlay(mrtTexture, debugLayerOpacity);
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			base.Start();

			blastoMrt = blasto.EnsureComponent<Mrt>();
			gastroMrt = gastro.EnsureComponent<Mrt>();

			childRenderers = GetComponentsInChildren<Renderer>();
		}

		protected new void OnDestroy() {
			base.OnDestroy();

			if(blastoMrt)
				Destroy(blastoMrt);
			if(gastroMrt)
				Destroy(gastroMrt);
		}

		protected void OnEnable() {
			mrtTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
			if(MainCamera.Instance)
				MainCamera.Instance.onRendered += OnRendered;
		}

		protected void OnDisable() {
			if(MainCamera.Instance)
				MainCamera.Instance.onRendered -= OnRendered;
			RenderTexture.ReleaseTemporary(mrtTexture);
			mrtTexture = null;
		}

		protected void Update() {
			visible = childRenderers.Any(r => r.isVisible);
		}
		#endregion
	}
}