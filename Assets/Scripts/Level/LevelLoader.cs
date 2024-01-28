using UnityEngine;

namespace NaniCore.Stencil {
	public class LevelLoader : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Level template;
		#endregion

		#region Interfaces
		public void InstantiateLevelAtPlace() {
			template.InstantiateLevelFromTemplate(transform);
		}

		public void InstantiateLevelAt(Transform place) {
			template.InstantiateLevelFromTemplate(place);
		}

		public void InstantiateLevelAtSpawnPoint() {
			template.InstantiateLevelFromTemplateAtSpawnPoint();
		}
		#endregion
	}
}
