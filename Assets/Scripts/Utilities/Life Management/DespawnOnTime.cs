using UnityEngine;
using UnityEngine.Events;

namespace NaniCore {
	public class DespawnOnTime : MonoBehaviour {
		#region Serialized Fields
		[SerializeField] private float lifeTime;
		[SerializeField] private UnityEvent onDespawn;
		#endregion

		#region Life cycle
		protected void Start() {
			StartCoroutine(DespawnOnTimeCoroutine());
		}
		#endregion

		#region Functions
		private System.Collections.IEnumerator DespawnOnTimeCoroutine() {
			yield return new WaitForSeconds(lifeTime);
			onDespawn?.Invoke();
			Destroy(gameObject);
		}
		#endregion
	}
}