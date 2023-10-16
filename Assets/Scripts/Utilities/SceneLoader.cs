using UnityEngine;

namespace NaniCore {
	public class SceneLoader : MonoBehaviour {
		#region Serialized properties
		public string sceneName;
		#endregion

		#region Functions
		public void LoadSingle() {
			SceneUtility.LoadSceneByNameAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
		}

		public void LoadAdditive() {
			SceneUtility.LoadSceneByNameAsync(sceneName,UnityEngine.SceneManagement.LoadSceneMode.Additive);
		}
		#endregion
	}
}