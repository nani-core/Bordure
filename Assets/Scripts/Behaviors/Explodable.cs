using UnityEngine;

namespace NaniCore.Bordure {
	public class Explodable : MonoBehaviour {
		#region Serialized fields
		[SerializeField][Min(0)] private float duration = .2f;
		[SerializeField][Min(0)] private float scale = 1.5f;
		[SerializeField] private GameObject afterEffect;
		#endregion

		#region Interfaces
		public void Explode() {
			StartCoroutine(ExplodeCoroutine());
		}
		#endregion

		#region Functions
		private System.Collections.IEnumerator ExplodeCoroutine() {
			Vector3 startScale = transform.localScale;
			yield return MathUtility.ProgressCoroutine(duration, t => {
				transform.localScale = startScale * Mathf.Lerp(1, scale, t);
			});
			if(afterEffect != null) {
				afterEffect = Instantiate(afterEffect);
				afterEffect.transform.AlignWith(transform);
			}
			gameObject.SetActive(false);
		}
		#endregion
	}
}
