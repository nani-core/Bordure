using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField][Expandable] private GameSettings settings;
		[SerializeField] private UiManager ui;
		[SerializeField] private EventSystem eventSystem;
		#endregion

		#region Interfaces
		public GameSettings Settings => settings;
		public UiManager Ui => ui;
		#endregion

		#region Life cycle
		protected void Awake() {
			if(!EnsureSingleton())
				return;

			Initialize();
		}

		protected void Update() {
			UpdateLoopShape();
			UpdateDebug();
		}

		protected void OnDestroy() {
			Finalize();
		}
		#endregion
	}
}