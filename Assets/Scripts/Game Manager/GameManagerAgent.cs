using UnityEngine;

namespace NaniCore.Bordure {
	using static GameManager;

	[CreateAssetMenu(menuName = "Nani Core/Game Manager Agent")]
	public class GameManagerAgent : ScriptableObject {
		#region Protagonist
		public void StartUsingProtagonist() {
			Instance.UsesProtagonist = true;
		}

		public void StopUsingProtagonist() {
			Instance.UsesProtagonist = false;
		}

		public void MoveProtagonistToSpawnPoint(SpawnPoint spawnPoint) {
			Instance.MoveProtagonistToSpawnPoint(spawnPoint);
		}

		public void MoveProtagonistToSpawnPointByName(string name) {
			Instance.MoveProtagonistToSpawnPointByName(name);
		}

		public bool UsesProtagonistMovement {
			get => Instance.UsesProtagonistMovement;
			set => Instance.UsesProtagonistMovement = value;
		}

		public bool UsesProtagonistOrientation {
			get => Instance.UsesProtagonistOrientation;
			set => Instance.UsesProtagonistOrientation = value;
		}

		public bool ProtagonistIsKinematic {
			get => Instance.ProtagonistIsKinematic;
			set => Instance.ProtagonistIsKinematic = value;
		}

		public void ProtagonistSitOn(Seat seat) {
			Instance.ProtagonistSitOn(seat);
		}

		public void ProtagonistLeaveSeat() {
			Instance.ProtagonistLeaveSeat();
		}
		#endregion

		#region Camera
		public void AlignCameraTo(Transform target) {
			Instance.AlignCameraTo(target);
		}

		public void AttachCameraTo(Transform transform) {
			Instance.AttachCameraTo(transform);
		}

		public void HardAttachCameraTo(Transform transform) {
			Instance.AttachCameraTo(transform, true);
		}

		public void RetrieveCameraHierarchy() {
			Instance.RetrieveCameraHierarchy();
		}

		public void TransitCameraTo(Transform target) {
			Instance.TransitCameraTo(target);
		}

		public void BlendToCamera(Camera target) {
			Instance.BlendToCamera(target);
		}

		public void BlendToCameraByName(string name) {
			Instance.BlendToCameraByName(name);
		}

		public void HardLookAt(Transform target) {
			Instance.HardLookAt(target);
		}
		#endregion

		#region Level
		public void LoadLevelByName(string name) {
			Instance.LoadLevelByName(name);
		}

		public void UnloadLevelByName(string name) {
			Instance.UnloadLevelByName(name);
		}

		public void AlignSpawnPoints(string names) {
			Instance.AlignSpawnPoints(names);
		}

		public void AddLevelLoadCallback(Logic logic) {
			Instance.AddLevelLoadCallback(logic);
		}

		public void DropLevelLoadCallbacks() {
			Instance.DropLevelLoadCallbacks();
		}
		#endregion

		#region Game
		public void StartGame() {
			Instance.StartGame();
		}

		public void QuitGame() {
			Instance.QuitGame();
		}

		public void RestartGame() {
			Instance.RestartGame();
		}

		public void FinishGame() {
			Instance.FinishGame();
		}

		public void UnloadStartScene() {
			Instance.UnloadStartScene();
		}
		#endregion

		#region Log
		public void PrintLog(string message) {
			Debug.Log(message);
		}
		#endregion

		#region UI
		public void OpenStartMenu() {
			Instance.PauseMenu.OpenStartMenu();
		}

		public void OpenSettings() {
			Instance.PauseMenu.OpenSettings();
		}

		public void OpenRestart() {
			Instance.PauseMenu.OpenRestart();
		}

		public void CloseLastUi() {
			Instance.PauseMenu.CloseLastUi();
		}
		#endregion

		#region Guidance
		public void ShowGuidance(string key) {
			Instance.ShowGuidance(key);
		}

		public void HideGuidance(string key) {
			Instance.HideGuidance(key);
		}
		#endregion

		#region Achievements
		public void FinishAchievement(string key) {
			Instance.FinishAchievement(key);
		}

		public void ResetAchievementProgress() {
			Instance.ResetAchievementProgress();
		}

		public void TriggerDuckAchievement(string key) {
			Instance.TriggerDuckAchievement(key);
		}

		public void TriggerLightOffAchievement(Light light) {
			Instance.TriggerLightOffAchievement(light);
		}
		#endregion
	}
}