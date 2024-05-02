namespace NaniCore.Bordure {
	public class FrequentLogic : Logic {
		public enum InvocationMode {
			Update, FixedUpdate,
		}
		public InvocationMode mode;

		protected void Update() {
			if(mode == InvocationMode.Update) {
				Invoke();
			}
		}

		protected void FixedUpdate() {
			if(mode == InvocationMode.FixedUpdate) {
				Invoke();
			}
		}
	}
}
