using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Fields
		private readonly List<Loopshape> loopshapes = new();
		private Loopshape[] validLoopshapes;
		#endregion

		#region Interfaces
		public Loopshape[] Loopshapes => loopshapes.Where(loopshape => loopshape.isActiveAndEnabled).ToArray();
		public Loopshape[] ValidLoopshapes => validLoopshapes;
		public bool HasValidLoopshapes => validLoopshapes != null && validLoopshapes?.Length > 0;
		#endregion

		#region Event handlers
		protected void OnLoopShapeCreated(Loopshape loopshape) {
			if(loopshape == null)
				return;

			if(!loopshapes.Contains(loopshape))
				loopshapes.Add(loopshape);
		}

		protected void OnLoopShapeDestroyed(Loopshape loopshape) {
			if(loopshape == null)
				return;

			if(loopshapes.Contains(loopshape))
				loopshapes.Remove(loopshape);
		}
		#endregion

		#region Life cycle
		protected void UpdateLoopShape() {
			loopshapes.RemoveAll(loopshape => loopshape == null);
			validLoopshapes = loopshapes.Where(loopshape => loopshape.IsValid).ToArray();
		}
		#endregion
	}
}