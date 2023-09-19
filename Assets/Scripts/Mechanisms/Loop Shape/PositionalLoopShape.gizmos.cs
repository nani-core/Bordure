#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NaniCore.Loopool {
	public partial class PositionalLoopShape : LoopShape {
		#region Functions
		private void DrawGastroAlongLine(float ratio) {
			if(blasto == null)
				return;
			if(gastro != null)
				GizmosUtility.DrawPhantom(gastro, GetPositionAlongViewingLine(ratio), Quaternion.Euler(0, placement.azimuth.pivot, 0) * Quaternion.LookRotation(BlastoPos - OriginPos));
			else
				Gizmos.DrawSphere(GetPositionAlongViewingLine(ratio), .0625f);
		}
		#endregion

		#region Life cycle
		protected void OnDrawGizmos() {
			if(!isActiveAndEnabled)
				return;
			bool isRelavantSelected = new GameObject[] { gameObject, gastro, blasto }.Any(go => Selection.Contains(go));
			bool isParentSelected = Selection.gameObjects.Any(go => transform.IsChildOf(go.transform));
			if(!(isRelavantSelected || isParentSelected))
				return;

			// Self-indicating ball
			{
				Color color = Color.red;
				color.a = .5f;
				Gizmos.color = color;
				Gizmos.DrawSphere(OriginPos, .125f);
			}

			if(blasto != null) {
				// View line to blasto
				{
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine(BlastoPos, OriginPos);
				}

				// Gastro phantom
				{
					Color idealColor = Color.green, extremeColor = Color.red;
					idealColor.a = .5f;
					extremeColor.a = .5f;

					Gizmos.color = idealColor;
					DrawGastroAlongLine(placement.distanceRatio.pivot);

					Gizmos.color = extremeColor;
					DrawGastroAlongLine(placement.distanceRatio.min);
					DrawGastroAlongLine(placement.distanceRatio.max);
				}

				// Positioning range
				Gizmos.color = Color.yellow;
				positioning.DrawGizmos(BlastoPos, Quaternion.LookRotation(OriginPos - BlastoPos), OriginPos);
			}
		}
		#endregion
	}
}
#endif