namespace NaniCore.Bordure {
	public class StartMenu : Menu {
		public override void OnEnter() {
			GameManager.Instance.Paused = true;
		}

		public override void OnExit() {
			GameManager.Instance.Paused = false;
		}
	}
}