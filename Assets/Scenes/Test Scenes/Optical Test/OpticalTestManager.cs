using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NaniCore.Bordure.OpticalTest {
	public class OpticalTestManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Camera mainCamera;
		[SerializeField] private MaskableGraphic stencilDisplay;
		#endregion

		#region Fields
		private RenderTexture stencilRt;
		#endregion

		#region Life cycle
		protected void Start() {
			//
		}

		protected void OnDestroy() {
			//stencilRt.Destroy();
		}
		#endregion
	}
}