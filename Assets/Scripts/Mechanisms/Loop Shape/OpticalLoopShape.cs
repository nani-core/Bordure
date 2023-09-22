using UnityEngine;

namespace NaniCore.Loopool {
	public class OpticalLoopShape : LoopShape {
		#region Serialized fields
		/// »·
		[SerializeField] private GameObject blasto;
		/// µº
		[SerializeField] private GameObject gastro;

		[SerializeField] [UnityEngine.Range(0, 1)] private float thickness;
		[SerializeField] [UnityEngine.Range(0, 1)] private float thicknessTolerance;
		#endregion

		#region Fields
		private Mrt blastoMrt, gastroMrt;
		private RenderTexture mrtTexture;
		private bool validated = false;
		#endregion

		#region Functions
		public override bool Validate(Transform eye) => validated;

		private void PerformValidation(RenderTexture cameraOutput) {
			if(blastoMrt == null || gastroMrt == null)
				return;

			mrtTexture.SetValue(Color.black);

			blastoMrt.RenderToTexture(mrtTexture);
			gastroMrt.RenderToTexture(mrtTexture);

			float standardHeight = 216f;
			var downsampled = mrtTexture.Resample(((Vector2)mrtTexture.Size() * (standardHeight / mrtTexture.height)).Floor());

			if(!downsampled.HasValue(gastroMrt.value)) {
				mrtTexture.ReplaceValueByValue(blastoMrt.value, Color.red);
				mrtTexture.ReplaceTextureByValue(Color.black, cameraOutput);
				return;
			}

			var gastroMask = downsampled.Duplicate();
			gastroMask.IndicateByValue(gastroMrt.value);

			var wholeMask = downsampled.Duplicate();
			wholeMask.ReplaceValueByValue(blastoMrt.value, Color.white);
			wholeMask.ReplaceValueByValue(gastroMrt.value, Color.white);
			wholeMask.IndicateByValue(Color.white);

			downsampled.Destroy();

			wholeMask.InfectByValue(Color.black, standardHeight * thickness);
			wholeMask.Difference(gastroMask);
			gastroMask.Destroy();

			var validationMask = wholeMask.Duplicate();
			validationMask.InfectByValue(Color.black, standardHeight * thicknessTolerance);
			var validated = !validationMask.HasValue(Color.white);
			validationMask.Destroy();

			wholeMask.ReplaceValueByValue(Color.white, validated ? Color.green : Color.red);

			wholeMask.filterMode = FilterMode.Point;
			Graphics.Blit(wholeMask, mrtTexture);
			wholeMask.Destroy();

			mrtTexture.ReplaceTextureByValue(Color.black, cameraOutput);
		}
		#endregion

		#region Message handlers
		private void OnPostFrameRender(Camera camera, RenderTexture cameraOutput) {
			PerformValidation(cameraOutput);
			Graphics.Blit(mrtTexture, cameraOutput);
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			base.Start();

			blastoMrt = blasto.GetComponent<Mrt>() ?? blasto.AddComponent<Mrt>();
			gastroMrt = gastro.GetComponent<Mrt>() ?? gastro.AddComponent<Mrt>();
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
		}

		protected void OnDisable() {
			RenderTexture.ReleaseTemporary(mrtTexture);
			mrtTexture = null;
		}

		protected void OnBecameVisible() {
			if(MainCamera.Instance)
				MainCamera.Instance.onPostFrameRender += OnPostFrameRender;
		}

		protected void OnBecameInvisible() {
			if(MainCamera.Instance)
				MainCamera.Instance.onPostFrameRender -= OnPostFrameRender;
			validated = false;
		}
		#endregion
	}
}