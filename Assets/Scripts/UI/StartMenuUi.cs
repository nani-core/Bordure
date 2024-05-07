namespace NaniCore.Bordure {
	public class StartMenuUi : Ui {
		public override void OnEnter() {
			GameManager.Instance.Paused = true;
		}

		public override void OnExit() {
			GameManager.Instance.Paused = false;
		}
	}
}