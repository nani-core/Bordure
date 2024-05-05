using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField][Expandable] private GameSettings settings;
		[SerializeField] private Level startLevel;
		#endregion

		#region Fields
		private bool gameStarted = false;
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

		public void StartGame() {
			gameStarted = true;
			CloseLastUi();
			UsesProtagonist = true;
		}

		public bool GameStarted => gameStarted;
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