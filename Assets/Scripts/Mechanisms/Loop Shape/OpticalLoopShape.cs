using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Loopool {
	public class OpticalLoopShape : LoopShape {
		const float standardHeight = 216f;

		#region Serialized fields
		[Header("Optical")]
		[SerializeField] protected GameObject blasto;
		[SerializeField] protected GameObject gastro;
		[SerializeField] [Range(0, 1)] private float thickness;
		[SerializeField] [Range(0, 1)] private float thicknessTolerance;
#if UNITY_EDITOR
		[SerializeField] private bool showDebugLayer = false;
#endif
		#endregion

		#region Fields
		private Mrt blastoMrt, gastroMrt;
		private RenderTexture mrtTexture;
		private bool validated = false;
		private bool visible = false;
		private IEnumerable<Renderer> childRenderers;
		#endregion

		#region Functions
		public override bool Validate(Transform eye) => validated;

		private bool ValidateByMask(RenderTexture gastroMask, RenderTexture wholeMask) {
			wholeMask.InfectByValue(Color.black, standardHeight * thickness);
			var intersect = wholeMask.Duplicate();
			intersect.Intersect(gastroMask);
			bool hasIntersection = intersect.HasValue(Color.white);
			intersect.Destroy();
			if(!hasIntersection)
				return false;

			wholeMask.Difference(gastroMask);

			var validationMask = wholeMask.Duplicate();
			validationMask.InfectByValue(Color.black, standardHeight * thicknessTolerance);
			var perfectlyMatched = !validationMask.HasValue(Color.white);
			validationMask.Destroy();
			return perfectlyMatched;
		}

		private bool PerformValidation(RenderTexture cameraOutput) {
			if(!visible || blastoMrt == null || gastroMrt == null)
				return false;

			mrtTexture.SetValue(Color.black);

			blastoMrt.RenderToTexture(mrtTexture);
			gastroMrt.RenderToTexture(mrtTexture);

			var downsampled = mrtTexture.Resample(((Vector2)mrtTexture.Size() * (standardHeight / mrtTexture.height)).Floor());

			if(!downsampled.HasValue(gastroMrt.value)) {
				mrtTexture.ReplaceValueByValue(blastoMrt.value, Color.red);
				mrtTexture.ReplaceTextureByValue(Color.black, cameraOutput);
				return false;
			}

			var gastroMask = downsampled.Duplicate();
			gastroMask.IndicateByValue(gastroMrt.value);

			var wholeMask = downsampled.Duplicate();
			wholeMask.ReplaceValueByValue(blastoMrt.value, Color.white);
			wholeMask.ReplaceValueByValue(gastroMrt.value, Color.white);
			wholeMask.IndicateByValue(Color.white);

			downsampled.Destroy();

			var validated = ValidateByMask(gastroMask, wholeMask);

			wholeMask.ReplaceValueByValue(Color.white, validated ? Color.green : Color.red);
			wholeMask.filterMode = FilterMode.Point;

			if(showDebugLayer) {
				Graphics.Blit(wholeMask, mrtTexture);
				mrtTexture.ReplaceTextureByValue(Color.black, cameraOutput);
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
		#endregion

		#region Message handlers
		private void OnPostFrameRender(Camera camera, RenderTexture cameraOutput) {
			if(!visible)
				return;
			validated = PerformValidation(cameraOutput);
			if(showDebugLayer) {
				Graphics.Blit(mrtTexture, cameraOutput);
			}
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			base.Start();

			blastoMrt = blasto.GetComponent<Mrt>() ?? blasto.AddComponent<Mrt>();
			gastroMrt = gastro.GetComponent<Mrt>() ?? gastro.AddComponent<Mrt>();

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
				MainCamera.Instance.onPostFrameRender += OnPostFrameRender;
		}

		protected void OnDisable() {
			if(MainCamera.Instance)
				MainCamera.Instance.onPostFrameRender -= OnPostFrameRender;
			RenderTexture.ReleaseTemporary(mrtTexture);
			mrtTexture = null;
		}

		protected void Update() {
			visible = childRenderers.Any(r => r.isVisible);
		}
		#endregion
	}
}