namespace NaniCore.Stencil {
	public class FocusValidator : LoopshapeValidator {
		protected override bool Validate() {
			if(!isActiveAndEnabled)
				return false;

			var protagonist = GameManager.Instance?.Protagonist;
			if(protagonist == null)
				return false;

			return protagonist.IsLookingAt(gameObject);
		}
	}
}