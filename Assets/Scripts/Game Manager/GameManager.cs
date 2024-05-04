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