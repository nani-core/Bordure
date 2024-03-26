using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	public class ElevatorButton : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Loopshape loopshape;
		#endregion

		#region Fields
		private Level level;
		#endregion

		#region Interfaces
		public UnityEvent OnClick => loopshape.onOpen;

		public Level Level {
			get => level;
			set {
				level = value;
				// TODO
			}
		}
		#endregion
	}
}
