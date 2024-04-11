using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NaughtyAttributes;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private Protagonist protagonist;
		[SerializeField][Label("Uses Protagonist at Start")] private bool usesProtagonist = true;
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

		public bool UsesProtagonist {
			get => usesProtagonist;
			set {
				if(usesProtagonist == value)
					return;
			}
		}
		#endregion

		#region Life cycles
		private void InitializeProtagonist() {
			protagonist = CreateProtagonist();
			if(true) {
				AttachCameraTo(Protagonist.Eye);
			}
		}

#if UNITY_EDITOR
		private void InitializeProtagonistInEditMode() {
			if(HierarchyUtility.IsInPrefabMode())
				return;

			MainCamera.transform.AlignWith(transform);
			protagonist = CreateProtagonist();
			if(Protagonist) {
				MainCamera.transform.AlignWith(Protagonist.Eye);
			}
		}
#endif

		private void FinalizeProtagonist() {
			DestroyProtagonist();
		}
		#endregion

		#region Functions
		private Protagonist CreateProtagonist() {
			List<Protagonist> existingProtagonists;
			{
				var runtime = new HashSet<Protagonist>(FindObjectsOfType<Protagonist>(true));
				if(!Application.isPlaying) {
					foreach(var protagonist in gameObject.FindAllComponentsInEditor<Protagonist>(true))
						runtime.Add(protagonist);
				}
				existingProtagonists = runtime.ToList();
			}
			if(startLevel != null) {
				Protagonist target;

				// Locate the target.
				{
					if(existingProtagonists.Count > 1) {
						Debug.LogWarning("Warning: Protagonist existing in scene is more than one, destroying all abundant ones.", this);
						foreach(var abundant in existingProtagonists.Skip(1))
							DestroyImmediate(abundant.gameObject);
					}
					if(existingProtagonists.Count > 0) {
						target = existingProtagonists[0];
						target.gameObject.RestorePrefabInstance();
					}
					else if(Settings.protagonist != null) {
						var newInstance = Settings.protagonist.gameObject.InstantiatePrefab();
						if(!newInstance.TryGetComponent(out target)) {
							Debug.LogWarning("Warning: There is no component of type Protagonist on the protagonist prefab.", Settings.protagonist);
							DestroyImmediate(newInstance);
						}
					}
					else
						target = null;
					// Could not locate or create a target by any means, aborting.
					if(target == null) {
						Debug.LogWarning("Warning: Could not instantiate protagonist in edit mode.", this);
						return null;
					}
				}

				// Assign the transform.
				{
					target.transform.SetParent(transform, true);
					SpawnPoint spawnPoint = startLevel.DebugSpawnPoint;
					target.transform.SetPositionAndRotation(
						spawnPoint.transform.position,
						Quaternion.LookRotation(spawnPoint.transform.forward)
					);

					target.gameObject.name = "Protagonist";
					if(!Application.isPlaying)
						target.gameObject.MakeUntouchable();
				}

				// Assign the profile.
				{
					if(Settings.protagonistProfile == null) {
						Debug.LogWarning("Warning: No protagonist profile is assigned.", Settings);
					}
					else {
						FieldInfo setter = typeof(Protagonist).GetField("profile",
							BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
						);
						setter.SetValue(target, Settings.protagonistProfile);
					}
				}

				return target;
			}
			else {
				if(existingProtagonists.Count > 0) {
					Debug.LogWarning("Warning: No start level assigned, removing existing protagonists.");
					foreach(var existing in existingProtagonists) {
						DestroyImmediate(existing.gameObject);
					}
				}
				return null;
			}
		}

		private void DestroyProtagonist() {
			if(Protagonist == null)
				return;
			try {
				// An exception might be raised on game exiting, as the game
				// manager which is the parent of the protaginst could be
				// pending to be destroyed.
				if(MainCamera.transform.IsChildOf(Protagonist.transform))
					RetrieveCameraHierarchy();
			} catch { }
			Destroy(Protagonist);
			protagonist = null;
		}
		#endregion
	}
}