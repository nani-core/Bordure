using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private Protagonist protagonist;
		#endregion

		#region Interfaces
		public Protagonist Protagonist {
			get {
				if(protagonist == null) {
					protagonist = null;
					return null;
				}
				return protagonist;
			}
		}

		public bool IsUsingProtagonist {
			get => Protagonist != null && Protagonist.isActiveAndEnabled;
			set {
				if(value == IsUsingProtagonist)
					return;
				if(value) {
					// Spawn the temporary camera anchor.
					var anchor = new GameObject().transform;
					anchor.SetParent(MainCamera.transform, false);
					anchor.SetParent(null, true);

					protagonist = GetProtagonistSingleton();
					Protagonist.gameObject.SetActive(true);
					AttachCameraTo(Protagonist.Eye, true);
					AlignCameraTo(anchor);

					Destroy(anchor.gameObject);
				}
				else {
					if(Protagonist == null)
						return;
					RetrieveCameraHierarchy();
					Protagonist.gameObject.SetActive(false);
				}
			}
		}

		public void MoveProtagonistToSpawnPoint(SpawnPoint spawnPoint) {
			if(!IsUsingProtagonist) {
				Debug.LogWarning("Warning: Cannot move the protagonist as we are not controlling it.");
				return;
			}
			Protagonist.transform.AlignWith(spawnPoint.transform);
		}

		public void MoveProtagonistToSpawnPointByName(string name) {
			var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
			foreach(var spawnPoint in spawnPoints) {
				if(spawnPoint.gameObject.name != name)
					continue;
				MoveProtagonistToSpawnPoint(spawnPoint);
				return;
			}
			Debug.LogWarning($"Warning: Cannot move the protagonist as the desired spawn point \"{name}\" does not exist.");
		}
		#endregion

		#region Functions
		private Protagonist CreateProtagonist() {
			var prefabInstance = Settings.protagonist.gameObject.InstantiatePrefab();
			if(!prefabInstance.TryGetComponent(out Protagonist newProtagonist)) {
				Debug.LogWarning("Warning: There is no component of type Protagonist on the protagonist prefab.", Settings.protagonist);
				HierarchyUtility.Destroy(prefabInstance);
				return null;
			}
			prefabInstance.name = "Protagonist";
			prefabInstance.transform.SetParent(transform);

			// Assign the profile.
			if(Settings.protagonistProfile == null) {
				Debug.LogWarning("Warning: No protagonist profile is assigned.", Settings);
			}

			// Make untouchable.
			if(!Application.isPlaying)
				newProtagonist.gameObject.MakeUntouchable();

			return newProtagonist;
		}

		private Protagonist GetProtagonistSingleton() {
			if(protagonist != null)
				return protagonist;
			protagonist = gameObject.GetComponentInChildren<Protagonist>(true);
			if(protagonist == null)
				protagonist = CreateProtagonist();

			return protagonist;
		}
		#endregion
	}
}