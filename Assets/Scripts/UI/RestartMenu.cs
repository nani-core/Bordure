namespace NaniCore.Bordure {
	public class RestartMenu : Menu {
		public override void OnEnter() {
			GameManager.Instance.UsesProtagonist = false;
			GameManager.Instance.Paused = true;
		}

		public override void OnExit() {
			GameManager.Instance.Paused = false;
			GameManager.Instance.UsesProtagonist = false;
		}
	}
}
