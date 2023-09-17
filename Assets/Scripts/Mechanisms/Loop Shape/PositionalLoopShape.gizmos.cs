#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Loopool {
	using CC = MathUtility.CylindricalCoordinate;

	public partial class PositionalLoopShape : LoopShape {
		#region Functions
		private void DrawGastroAlongLine(float ratio) {
			if(blasto == null)
				return;
			if(gastro != null)
				gastro.DrawPhantom(GetPositionAlongViewingLine(ratio), IdealPlacementRotation);
			else
				Gizmos.DrawSphere(GetPositionAlongViewingLine(ratio), .0625f);
		}
		#endregion

		#region Life cycle
		protected void OnDrawGizmos() {
			// Self-indicating ball
			{
				Color color = Color.red;
				color.a = .5f;
				Gizmos.color = color;
				Gizmos.DrawSphere(InitialViewPosition, .125f);
			}

			if(blasto != null) {
				// View line to blasto
				{
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine(BlastoPosition, InitialViewPosition);
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
				{
					var azimuthRadian = new FloatRange(positioning.azimuth);
					azimuthRadian.min *= Mathf.PI / 180;
					azimuthRadian.max *= Mathf.PI / 180;
					var distanceRatio = positioning.distanceRatio;
					var rawVertices = new List<CC> {
						new CC(distanceRatio.min, azimuthRadian.min, 0),
						new CC(distanceRatio.min, 0, 0),
						new CC(distanceRatio.min, azimuthRadian.max, 0),
						new CC(distanceRatio.max, azimuthRadian.max, 0),
						new CC(distanceRatio.max, 0, 0),
						new CC(distanceRatio.max, azimuthRadian.min, 0),
					};
					Gizmos.color = Color.yellow;
					GizmosUtility.DrawPolygon(rawVertices.Select(cc => {
						cc.radius *= InitialViewDirection.magnitude;
						return BlastoToWorldMatrix.MultiplyPoint((Vector3)cc);
					}));
				}
			}
		}
		#endregion
	}
}
#endif