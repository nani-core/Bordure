namespace NaniCore.Bordure {
	public class RestartMenu : Menu {
		public override void OnEnter() {
			GameManager.Instance.UsesProtagonist = false;
		}

		public override void OnExit() {
			GameManager.Instance.UsesProtagonist = false;
		}
	}
}
