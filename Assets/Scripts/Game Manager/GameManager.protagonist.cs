using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace NaniCore.Loopool {
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
		public Camera MainCamera => Protagonist?.Camera;
		#endregion

		#region Functions
		private Protagonist InitializeProtagonist() {
			List<Protagonist> existingProtagonists;
			{
				var runtime = new HashSet<Protagonist>(FindObjectsOfType<Protagonist>(true));
				if(!Application.isPlaying) {
					foreach(var protagonist in gameObject.FindAllComponentsInEditor<Protagonist>(true))
						runtime.Add(protagonist);
				}
				existingProtagonists = runtime.ToList();
			}
			if(spawnPoint != null) {
				Protagonist target;

				// Locate the target.
				{
					if(existingProtagonists.Count > 1) {
						Debug.LogWarning("Warning: Protagonist existing in scene is more than one, destroying all abundant ones.", this);
						foreach(var abundant in existingProtagonists.Skip(1))
							DestroyImmediate(abundant.gameObject);
					}
					if(existingProtagonists.Count > 0)
						target = existingProtagonists[0];
					else
						target = null;
					// If none is suitable, create one.
					if(target == null) {
						if(Settings.protagonist != null) {
							var newInstance = Settings.protagonist.gameObject.InstantiatePrefab();
							target = newInstance.GetComponent<Protagonist>();
							if(target == null) {
								Debug.LogWarning("Warning: There is no component of type Protagonist on the protagonist prefab.", Settings.protagonist);
								DestroyImmediate(newInstance);
							}
						}
					}
					// Could not locate or create a target by any means, aborting.
					if(target == null) {
						Debug.LogWarning("Warning: Could not instantiate protagonist in edit mode.", this);
						return null;
					}
				}

				// Assign the transform.
				{
					target.transform.SetParent(transform, true);
					target.transform.position = spawnPoint.position;
					target.transform.rotation = Quaternion.LookRotation(spawnPoint.forward);

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
					Debug.LogWarning("Warning: No spawn point assigned, removing existing protagonists.");
					foreach(var existing in existingProtagonists) {
						DestroyImmediate(existing.gameObject);
					}
				}
				return null;
			}
		}
		#endregion
	}
}