using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField][Expandable] private GameSettings settings;
		[SerializeField] private Level startLevel;
		#endregion

		#region Interfaces
		public GameSettings Settings => settings;

		public bool Paused {
			get => Time.timeScale > 0.0f;
			set {
				if(value) {
					if(Protagonist != null) {
						wasUsingProtagonist = UsesProtagonist;
						Protagonist.enabled = false;
					}
					TimeScale = 0.0f;
				}
				else {
					TimeScale = 1.0f;
					if(Protagonist != null) {
						Protagonist.enabled = wasUsingProtagonist;
					}
				}
			}
		}
		#endregion

		#region Life cycle
		protected void Awake() {
			if(!EnsureSingleton())
				return;

			Initialize();
		}

		protected void Update() {
			UpdateLoopShape();
			UpdateUi();
		}

		protected void OnDestroy() {
			Finalize();
		}
		#endregion
	}
}