using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public static class SceneUtility {
	public static AsyncOperation LoadSceneByNameAsync(string name, LoadSceneMode mode) {
		var parameters = new LoadSceneParameters {
			loadSceneMode = mode,
		};
#if UNITY_EDITOR
		return EditorSceneManager.LoadSceneAsyncInPlayMode($"Assets/Scenes/{name}.unity", parameters);
#else
		return SceneManager.LoadSceneAsync(name, parameters);
#endif
	}
}