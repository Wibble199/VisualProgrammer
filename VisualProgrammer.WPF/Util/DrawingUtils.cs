using System.Windows;
using System.Windows.Media;

namespace VisualProgrammer.WPF.Util {

	public static class DrawingUtils {

		/// <summary>
		/// Draws a curved line suitable for vertical or horizontal connections between two points on the given drawing context.
		/// </summary>
		/// <param name="context">The drawing context to draw the line to.</param>
		/// <param name="pen">The pen with which to draw the line with.</param>
		/// <param name="startPoint">The initial start point of the line.</param>
		/// <param name="endPoint">The end/destination point of the line.</param>
		/// <param name="isVertical">Determines whether the values the curve points will be set to will make the line suitable for
		/// connecting two areas vertically (<c>true</c>) or horizontally (<c>false</c>).</param>
		public static void DrawConnectorLine(this DrawingContext context, Pen pen, Point startPoint, Point endPoint, bool isVertical) {
			// Bezier control points
			var cp1 = isVertical ? new Point(startPoint.X, (startPoint.Y + endPoint.Y) / 2) : new Point((startPoint.X + endPoint.X) / 2, startPoint.Y);
			var cp2 = isVertical ? new Point(endPoint.X, (startPoint.Y + endPoint.Y) / 2) : new Point((startPoint.X + endPoint.X) / 2, endPoint.Y);

			context.DrawGeometry(Brushes.Transparent, pen, new PathGeometry(new[] {
				new PathFigure(
					startPoint,
					new[] { new BezierSegment(cp1, cp2, endPoint, true) },
					false
				)
			}));
		}

	}
}
