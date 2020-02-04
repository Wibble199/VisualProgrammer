using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.AttachedProperties;
using VisualProgrammer.WPF.Util;
using VisualProgrammer.WPF.ViewModels;

namespace VisualProgrammer.WPF {

	/// <summary>
	/// A control that represents a connector attached to a visual node which the user can drag-drop onto another connector to form a link between nodes.<para/>
	/// Note that this connector will automatically attach events to itself for starting/ending the drag on the <see cref="VisualNodeCanvas"/> ancestor.
	/// </summary>
	public class VisualNodeConnector : Control {

		static VisualNodeConnector() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodeConnector), new FrameworkPropertyMetadata(typeof(VisualNodeConnector)));
		}

		public VisualNodeConnector() {
			MouseDown += StartConnectorDrag;
			MouseUp += EndConnectorDrag;
		}

		/// <summary>Gets the midpoint of this connector (relative to this visual element).</summary>
		public Point MidPoint => new Point(ActualWidth / 2, ActualHeight / 2);

		#region NodeID DependencyProperty
		/// <summary>
		/// The ID of the node that this connector is attached to.
		/// </summary>
		public Guid NodeID {
			get => (Guid)GetValue(NodeIDProperty);
			set => SetValue(NodeIDProperty, value);
		}

		public static readonly DependencyProperty NodeIDProperty =
			DependencyProperty.Register("NodeID", typeof(Guid), typeof(VisualNodeConnector), new PropertyMetadata(Guid.Empty));
		#endregion

		#region PropertyName DependencyProperty
		/// <summary>
		/// The name of the property or statement that this connector represents.<para/>
		/// If this connector is the input node of a statement or the output node of an expression, this will be an empty string.
		/// </summary>
		public string PropertyName {
			get => (string)GetValue(PropertyNameProperty);
			set => SetValue(PropertyNameProperty, value);
		}

		public static readonly DependencyProperty PropertyNameProperty =
			DependencyProperty.Register("PropertyName", typeof(string), typeof(VisualNodeConnector), new PropertyMetadata(""));
		#endregion

		#region ConnectorFlow DependencyProperty
		/// <summary>
		/// The direction of data flow from this connector.
		/// </summary>
		public ConnectorFlow ConnectorFlow {
			get => (ConnectorFlow)GetValue(ConnectorFlowProperty);
			set => SetValue(ConnectorFlowProperty, value);
		}

		public static readonly DependencyProperty ConnectorFlowProperty =
			DependencyProperty.Register("ConnectorFlow", typeof(ConnectorFlow), typeof(VisualNodeConnector), new PropertyMetadata(ConnectorFlow.Destination));
		#endregion

		#region Event Handlers
		private void StartConnectorDrag(object sender, MouseButtonEventArgs e) =>
			VisualProgramViewModelAttachedProperty.GetVisualProgramModel(this).DragBehaviour = new ConnectorDragBehaviour(this);

		private void EndConnectorDrag(object sender, MouseButtonEventArgs e) {
			var vpvm = VisualProgramViewModelAttachedProperty.GetVisualProgramModel(this);
			if (!(vpvm.DragBehaviour is ConnectorDragBehaviour cdb)) return;
			ConnectTo(vpvm.model, cdb.Connector);
		}
		#endregion

		/// <summary>
		/// Attempts to create a link in the given <see cref="VisualProgram"/> context from this connector to another connector.<para/>
		/// Does not catch any exceptions thrown by <see cref="VisualNode.Link(VisualProgram, string, VisualNode)"/>.
		/// </summary>
		/// <param name="programContext">The program in which the link will be created.</param>
		/// <param name="other">The other connector that should be connected to this one.</param>
		/// <exception cref="VisualNodeLinkException">Forwarded from <see cref="VisualNode.Link(VisualProgram, string, VisualNode)"/>.</exception>
		/// <seealso cref="VisualNode.Link(VisualProgram, string, VisualNode)"/>
		public void ConnectTo(VisualProgram programContext, VisualNodeConnector other) {
			// Perform a quick initial check to see if we maybe can connect the two connectors
			if (other == null
			 || NodeID == other.NodeID
			 || ConnectorFlow == other.ConnectorFlow) return;

			// Figure out which of the drag ends is the input/output of the connection
			var parentData = ConnectorFlow == ConnectorFlow.Destination ? this : other;
			var childData = ConnectorFlow == ConnectorFlow.Source ? this : other;

			try {
				// Perform the link
				programContext.Nodes[parentData.NodeID].Link(programContext, parentData.PropertyName, programContext.Nodes[childData.NodeID]);
				DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this)?.InvalidateVisual(); // Indicate that the canvas needs to be redrawn with a new connection line

			} catch { }
		}
	}


	/// <summary>
	/// The data flow of a connector, either in or out.
	/// </summary>
	public enum ConnectorFlow {
		/// <summary>This connector represents a connection that expects an outgoing return value or statement flow.</summary>
		Source,

		/// <summary>This connector represents a connection that expects an incoming parameter value or previous statement reference.</summary>
		Destination
	}


	internal class ConnectorDragBehaviour : ICanvasDragBehaviour {

		private Point lastPoint;
		private readonly bool isStatement;
		private readonly Pen linePen;

		public ConnectorDragBehaviour(VisualNodeConnector source) {
			Connector = source;

			var vm = source.DataContext as VisualNodeViewModel;
			isStatement = vm.IsStatement;
			linePen = new Pen(new SolidColorBrush(isStatement ? Colors.Gray : source.ColorForDataType(vm.ExpressionType)), 2d);
		}

		public VisualNodeConnector Connector { get; }

		public bool MoveStep(MouseEventArgs e, Point canvasPoint) {
			lastPoint = canvasPoint;
			return true;
		}

		public bool StopMove(MouseEventArgs e, Point canvasPoint) => true;

		public void OnRender(VisualNodeCanvas canvas, DrawingContext drawingContext) {
			drawingContext.DrawConnectorLine(
				linePen,
				Connector.TransformToAncestor(canvas).Transform(Connector.MidPoint),
				lastPoint,
				isStatement
			);
		}
	}
}
