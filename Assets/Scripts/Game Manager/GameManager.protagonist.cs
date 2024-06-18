using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Fields
		private Protagonist protagonist;
		private Seat currentSeat;
		[System.NonSerialized] public float mouseSensitivityGain = 1.0f;
		#endregion

		#region Interfaces
		public System.Action onProtagonistEnabled, onProtagonistDisabled;

		public Protagonist Protagonist {
			get {
				if(protagonist == null) {
					// To flush invalidated Unity references.
					protagonist = null;
					return null;
				}
				return protagonist;
			}
		}

		public bool UsesProtagonist {
			get => Protagonist != null && Protagonist.isActiveAndEnabled;
			set {
				if(value) {
					// Spawn the temporary camera anchor.
					var anchor = new GameObject().transform;
					anchor.SetParent(MainCamera.transform, false);
					anchor.SetParent(null, true);

					protagonist = GetProtagonistSingleton();
					Protagonist.enabled = true;
					AttachCameraTo(Protagonist.Eye, true);
					AlignCameraTo(anchor);

					Destroy(anchor.gameObject);

					onProtagonistEnabled?.Invoke();
				}
				else {
					if(Protagonist == null)
						return;
					RetrieveCameraHierarchy();
					Protagonist.enabled = false;

					onProtagonistDisabled?.Invoke();
				}
			}
		}

		public void MoveProtagonistToSpawnPoint(SpawnPoint spawnPoint) {
			if(spawnPoint == null) {
				Debug.LogWarning("Warning: Trying to move the protagonist to an empty spawn point, aborting.");
				return;
			}
			var levelSection = spawnPoint.transform.GetComponentInParent<LevelSection>(true);
			if(levelSection != null)
				levelSection.Load();

			if(UsesProtagonist) {
				Protagonist.transform.AlignWith(spawnPoint.transform);
			}
			else {
				AlignCameraTo(spawnPoint.transform);
				var profile = Settings.protagonistProfile;
				float dy = profile.height - profile.eyeHanging;
				MainCamera.transform.Translate(Vector3.up * dy);
			}
		}

		public void MoveProtagonistToSpawnPointByName(string name) {
			var spawnPoint = FindSpawnPointByName(name);
			if(spawnPoint == null) {
				Debug.LogWarning($"Warning: Cannot move the protagonist as the desired spawn point \"{name}\" does not exist.");
				return;
			}
			MoveProtagonistToSpawnPoint(spawnPoint);
		}

		public bool UsesProtagonistMovement {
			get => Protagonist?.UsesMovement ?? false;
			set {
				if(Protagonist == null)
					return;
				Protagonist.UsesMovement = value;
			}
		}

		public bool UsesProtagonistOrientation {
			get => Protagonist?.UsesOrientation ?? false;
			set {
				if(Protagonist == null)
					return;
				Protagonist.UsesOrientation = value;
			}
		}

		public bool ProtagonistIsKinematic {
			get => Protagonist?.IsKinematic ?? false;
			set {
				if(Protagonist == null)
					return;

				Protagonist.IsKinematic = value;
			}
		}

		public Seat CurrentSeat => currentSeat;

		public void ProtagonistSitOn(Seat seat) {
			if(seat == null)
				return;

			ProtagonistIsKinematic = true;
			UsesProtagonistMovement = false;
			UsesProtagonistOrientation = seat.canOrient;

			TransitCameraTo(seat.transform);

			currentSeat = seat;
			currentSeat.SendMessage("OnSitOn", SendMessageOptions.DontRequireReceiver);
		}

		public void ProtagonistLeaveSeat() {
			if(currentSeat == null)
				return;

			ProtagonistIsKinematic = false;
			UsesProtagonistMovement = true;
			UsesProtagonistOrientation = true;

			currentSeat.SendMessage("OnLeft", SendMessageOptions.DontRequireReceiver);
			currentSeat = null;
		}

		public float MouseSensitivityGainInExponent {
			get => Mathf.Log(mouseSensitivityGain);
			set => mouseSensitivityGain = Mathf.Exp(value);
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