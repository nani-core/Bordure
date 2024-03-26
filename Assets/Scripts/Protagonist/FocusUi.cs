using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.Port;

namespace NaniCore.Bordure {
	public class FocusUi : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Image image;
		[Serializable]
		public struct FocusRadiusMap {
			public float Normal;
			public float Hovering;
			public float Grabbing;
		}
		[SerializeField] private FocusRadiusMap focusRadiusMap;
		[SerializeField] private float speed = 1f;
		#endregion

		#region Fields
		private Material material;
		public enum Status { Normal = 0, Hovering, Grabbing, }
		private Status currentStatus = Status.Normal;
		private float radius;
		private float opacity = 1f;
		#endregion

		#region Interfaces
		public Status CurrentStatus {
			get => currentStatus;
			set {
				if(currentStatus == value)
					return;
				currentStatus = value;
				AnimateZooming();
			}
		}

		public float Radius {
			get => radius;
			set {
				radius = value;
				material.SetFloat("_Radius", radius);
			}
		}

		public float Opacity {
			get => opacity;
			set {
				opacity = value;
				material.SetFloat("_Opacity", opacity);
			}
		}
		#endregion

		#region Functions
		private void AnimateZooming() {
			switch(currentStatus) {
				case Status.Normal:
					ZoomCubic(focusRadiusMap.Normal);
					break;
				case Status.Hovering:
					ZoomCubic(focusRadiusMap.Hovering);
					break;
				case Status.Grabbing:
					ZoomCubic(focusRadiusMap.Grabbing);
					break;
			}
		}

		private Coroutine currCoroutine;
		private void ZoomCubic(float newr) {
			if(currCoroutine != null)
				StopCoroutine(currCoroutine);
			currCoroutine = StartCoroutine(IZoomCubic(newr));
		}

		private IEnumerator IZoomCubic(float rNew) {
			float rOld = Radius;
			for(float x = 0f; x < 1f; x += speed * Time.deltaTime) {
				float y = 5.093f * x * x * x - 10.231f * x * x + 6.139f * x;
				Radius = rOld + (rNew - rOld) * y;
				yield return new WaitForEndOfFrame();
			}
			Radius = rNew;
		}
		#endregion

		#region Life cycle
		protected void Start() {
			image.material = material = new Material(image.material);
			Radius = 0;
			AnimateZooming();
		}
		#endregion
	}
}
