using System.Windows;
using System.Windows.Input;

namespace VisualProgrammer.WPF.Util {

	/// <summary>
	/// An interface that defines a behaviour for a specific type of drag action.
	/// </summary>
	internal interface ICanvasDragBehaviour {
		/// <summary>
		/// Whether or not the mouse should be captured by the canvas when this behaviour is active.
		/// </summary>
		bool ShouldCapture => false;

		/// <summary>
		/// Behaviour for when the user moves the mouse while this drag behaviour is active.
		/// </summary>
		/// <param name="e">The mouse event that caused the step.</param>
		/// <param name="canvasPoint">The mouse position relative to the canvas.</param>
		/// <returns>Returns whether or not the canvas should invalidate.</returns>
		bool MoveStep(MouseEventArgs e, Point canvasPoint);

		/// <summary>
		/// Behaviour for when the user releases the mouse while this drag behaviour is active.
		/// </summary>
		/// <param name="e">The mouse event that caused the step.</param>
		/// <param name="canvasPoint">The mouse position relative to the canvas.</param>
		/// <returns>Returns whether or not the canvas should invalidate.</returns>
		bool StopMove(MouseEventArgs e, Point canvasPoint);

		/// <summary>
		/// Method that will be called at the end of the Canvas's OnRender method.
		/// </summary>
		/// <param name="canvas">The canvas that triggered the render.</param>
		/// <param name="drawingContext">The drawing context of the canvas.</param>
		void OnRender(VisualNodeCanvas canvas, System.Windows.Media.DrawingContext drawingContext) { }
	}
}
