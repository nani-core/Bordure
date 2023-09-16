#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NaniCore.Loopool {
	public partial class PositionalLoopShape : LoopShape {
		#region Life cycle
		protected void OnDrawGizmos() {
			// Self-indicating ball
			{
				Color color = Color.red;
				color.a = .5f;
				Gizmos.color = color;
				Gizmos.DrawSphere(transform.position, .125f);
			}

			if(blasto != null) {
				// View line to blasto
				{
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine(blasto.transform.position, transform.position);
				}

				// Gastro phantom
				{
					Color phantomColor = Color.yellow;
					phantomColor.a = .5f;
						Gizmos.color = phantomColor;
					if(gastro != null)
						gastro.DrawPhantom(IdealPosition, IdealRotation);
					else
						Gizmos.DrawSphere(IdealPosition, .0625f);
				}
			}
		}
		#endregion
	}
}
#endif