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
					protagonist = GetProtagonistSingleton();
					Protagonist.gameObject.SetActive(true);
					AttachCameraTo(Protagonist.Eye, true);
				}
				else {
					if(Protagonist == null)
						return;
					Protagonist.gameObject.SetActive(false);
				}
			}
		}
		#endregion

		#region Life cycles
		private void InitializeProtagonist() {
			IsUsingProtagonist = startLevel.spawnProtagonistAtStart;
		}

		private void FinalizeProtagonist() {
			DestroyProtagonist();
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

			// Assign the profile.
			if(Settings.protagonistProfile == null) {
				Debug.LogWarning("Warning: No protagonist profile is assigned.", Settings);
			}

			// Make untouchable.
			if(!Application.isPlaying)
				newProtagonist.gameObject.MakeUntouchable();

			// Move to the spawn point.
			if(startLevel != null) {
				newProtagonist.transform.SetParent(transform, true);
				SpawnPoint spawnPoint = startLevel.DebugSpawnPoint;
				newProtagonist.transform.SetPositionAndRotation(
					spawnPoint.transform.position,
					Quaternion.LookRotation(spawnPoint.transform.forward)
				);
			}

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

		private void DestroyProtagonist() {
			if(Protagonist == null)
				return;
			if(!IsBeingDestroyed) {
				// An exception might be raised on game exiting, as the game
				// manager which is the parent of the protaginst could be
				// pending to be destroyed.
				if(MainCamera.transform.IsChildOf(Protagonist.transform))
					RetrieveCameraHierarchy();
			}
			HierarchyUtility.Destroy(Protagonist);
			protagonist = null;
		}
		#endregion
	}
}