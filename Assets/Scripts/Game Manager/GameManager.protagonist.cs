using UnityEngine;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private Protagonist protagonist;
		#endregion

		#region Interfaces
		public Protagonist Protagonist => protagonist;
		public Camera MainCamera => Protagonist?.Camera;

		public bool RegisterProtagonist(Protagonist protagonist) {
			if(this.protagonist != null && this.protagonist != protagonist)
				return false;

			this.protagonist = protagonist;
			return true;
		}

		public void UnregisterProtagonist(Protagonist protagonist) {
			if(protagonist != this.protagonist)
				return;
			this.protagonist = null;
		}
		#endregion
	}
}