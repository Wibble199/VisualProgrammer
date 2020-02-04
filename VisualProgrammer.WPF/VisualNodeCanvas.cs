using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.AttachedProperties;
using VisualProgrammer.WPF.Util;
using VisualProgrammer.WPF.ViewModels;

namespace VisualProgrammer.WPF {

	public class VisualNodeCanvas : ItemsControl {

        static VisualNodeCanvas() {
            // Indicate that we need to use our custom style (which is based off the default ItemsControl style)
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodeCanvas), new FrameworkPropertyMetadata(typeof(VisualNodeCanvas)));
        }

		public VisualNodeCanvas() {
			// For some reason the lines do not appear when the control is first created, even though
			// a breakpoint on DrawLine is hit before the control appears, so it should be drawing it?
			// So, as a hacky work around, we can trigger another re-render just after the control has loaded
			Loaded += (sender, e) => InvalidateVisual();

			// Drag-related handlers
			MouseMove += StepDrag;
			MouseUp += EndDrag;

			// Routed Commands
			CommandBindings.Add(new CommandBinding(Commands.StartMove, NodePresenterStartMove));
		}

		internal VisualProgramViewModel ViewModel => VisualProgramViewModelAttachedProperty.GetVisualProgramModel(this);

		protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

			// Draw lines between connectors
			if (ViewModel?.Nodes != null) {
				foreach (var node in ViewModel.Nodes) {
					foreach (var prop in node.LinkableProperties) {
						// Check if the property has a reference to an expression, if so, find the connector points
						if (prop.Value is INodeReference nr && nr.Id.HasValue) {
							var start = DependencyObjectUtils.ChildOfType<VisualNodeConnector>(this, c => c.NodeID == node.ID && c.PropertyName == prop.Name && c.ConnectorFlow == ConnectorFlow.Destination);
							var end = DependencyObjectUtils.ChildOfType<VisualNodeConnector>(this, c => c.NodeID == nr.Id.Value && c.ConnectorFlow == ConnectorFlow.Source);
							var isStatement = prop.PropertyType == VisualNodePropertyType.Statement;

							if (start != null && end != null)
								drawingContext.DrawConnectorLine(
									new Pen(new SolidColorBrush(isStatement ? Colors.Gray : this.ColorForDataType(prop.Type)), 2d),
									start.TransformToAncestor(this).Transform(start.MidPoint),
									end.TransformToAncestor(this).Transform(end.MidPoint),
									isStatement
								);
						}
					}
				}
			}

			// If there is a drag going on, run the behaviours custom render method
			ViewModel?.DragBehaviour?.OnRender(this, drawingContext);
        }

		/// <summary>
		/// Command handler for when a node presenters drag starts.
		/// </summary>
		private void NodePresenterStartMove(object sender, ExecutedRoutedEventArgs e) {
			if (!(e.OriginalSource is VisualNodePresenter presenter && e.Parameter is Point p)) return;
			var vm = (VisualNodeViewModel)presenter.DataContext;
			ViewModel.DragBehaviour = new VisualNodePresenterDragBehaviour(vm, p);
		}

		/// <summary>
		/// Steps the drag action.
		/// </summary>
		private void StepDrag(object sender, MouseEventArgs e) {
			if (ViewModel.DragBehaviour?.MoveStep(e, e.GetPosition(this)) == true)
				DispatchDelayedInvalidate();
			if (ViewModel.DragBehaviour?.ShouldCapture == true && !IsMouseCaptured)
				CaptureMouse();
		}

		/// <summary>
		/// Finishes the drag action.
		/// </summary>
		private void EndDrag(object sender, MouseEventArgs e) {
			if (ViewModel.DragBehaviour?.StopMove(e, e.GetPosition(this)) == true)
				DispatchDelayedInvalidate();
			ViewModel.DragBehaviour = null;
			ReleaseMouseCapture();
		}

		/// <summary>
		/// Executes a InvalidateArrange and InvalidateVisual call when the dispatcher's context is next idle.
		/// </summary>
		private void DispatchDelayedInvalidate() {
			// Updating the position will cause the node presenter to re-render. We need to wait for it to have it's new layout
			// before we cause a line re-rendering (which we trigger by InvalidateVisual) since the line renderer fetches the
			// position of the connectors when drawing the lines.
			Dispatcher.Invoke(() => {
				InvalidateArrange();
				InvalidateVisual();
			}, DispatcherPriority.ContextIdle);
		}
	}
}
