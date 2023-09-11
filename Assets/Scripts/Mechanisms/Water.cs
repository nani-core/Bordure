using UnityEngine;
using System.Collections;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(BoxCollider))]
	public partial class Water : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Transform surface;
		[SerializeField][Min(0)] private float height = 1;
		[SerializeField][Min(0)] private float speed = 1;
		#endregion

		#region Fields
		private Coroutine targetHeightCoroutine;
		#endregion

		#region Functions
		private BoxCollider Collider => GetComponent<BoxCollider>();

		private float Height {
			get => height;
			set {
				Vector3 center = Collider.center;
				center.y = value * .5f;
				Collider.center = center;
				Collider.size = new Vector3(1, value, 1);
				surface.localPosition = Vector3.up * value;
				height = value;
			}
		}

		public float TargetHeight {
			set {
				if(targetHeightCoroutine != null)
					StopCoroutine(targetHeightCoroutine);
				targetHeightCoroutine = StartCoroutine(SetTargetHeightCoroutine(value));
			}
		}

		private IEnumerator SetTargetHeightCoroutine(float value) {
			if(Height == value)
				yield break;
			float height = Height;
			float sgn = Mathf.Sign(value - height);
			while(true) {
				height += sgn * speed * Time.fixedDeltaTime;
				if(Mathf.Sign(value - height) * sgn <= 0)
					break;
				Height = height;
				yield return new WaitForFixedUpdate();
			}
			Height = value;
			targetHeightCoroutine = null;
		}
		#endregion
	}
}