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
		protected void OnValidate() {
			if(allowedPlacementDistanceRatio.x > idealPlacementDistanceRatio)
				allowedPlacementDistanceRatio.x = idealPlacementDistanceRatio;
			if(allowedPlacementDistanceRatio.y < idealPlacementDistanceRatio)
				allowedPlacementDistanceRatio.y = idealPlacementDistanceRatio;
		}

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
					DrawGastroAlongLine(idealPlacementDistanceRatio);

					Gizmos.color = extremeColor;
					DrawGastroAlongLine(allowedPlacementDistanceRatio.x);
					DrawGastroAlongLine(allowedPlacementDistanceRatio.y);
				}

				// Positioning range
				{
					Vector2 allowedPositioningAzimuthRadian = allowedPositioningAzimuth * Mathf.PI / 180;
					var rawVertices = new List<CC> {
						new CC(allowedPositioningDistanceRatio.x, allowedPositioningAzimuthRadian.x, 0),
						new CC(allowedPositioningDistanceRatio.x, 0, 0),
						new CC(allowedPositioningDistanceRatio.x, allowedPositioningAzimuthRadian.y, 0),
						new CC(allowedPositioningDistanceRatio.y, allowedPositioningAzimuthRadian.y, 0),
						new CC(allowedPositioningDistanceRatio.y, 0, 0),
						new CC(allowedPositioningDistanceRatio.y, allowedPositioningAzimuthRadian.x, 0),
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