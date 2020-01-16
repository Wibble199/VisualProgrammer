using System;
using System.Windows;
using System.Windows.Controls;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.Util;

namespace VisualProgrammer.WPF {

	public class VisualNodeConnector : Control {

		static VisualNodeConnector() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodeConnector), new FrameworkPropertyMetadata(typeof(VisualNodeConnector)));
		}

		#region Node DependencyProperty
		public VisualNode Node {
			get { return (VisualNode)GetValue(NodeProperty); }
			set { SetValue(NodeProperty, value); }
		}

		public static readonly DependencyProperty NodeProperty =
			DependencyProperty.Register("Node", typeof(VisualNode), typeof(VisualNodeConnector), new PropertyMetadata(null));
		#endregion

		#region NodeID DependencyProperty
		public Guid NodeID {
			get => (Guid)GetValue(NodeIDProperty);
			set => SetValue(NodeIDProperty, value);
		}

		public static readonly DependencyProperty NodeIDProperty =
			DependencyProperty.Register("NodeID", typeof(Guid), typeof(VisualNodeConnector), new PropertyMetadata(Guid.Empty));
		#endregion

		#region PropertyName DependencyProperty
		public string PropertyName {
			get => (string)GetValue(PropertyNameProperty);
			set => SetValue(PropertyNameProperty, value);
		}

		public static readonly DependencyProperty PropertyNameProperty =
			DependencyProperty.Register("PropertyName", typeof(string), typeof(VisualNodeConnector), new PropertyMetadata(""));
		#endregion

		#region ConnectorFlow DependencyProperty
		public ConnectorFlow ConnectorFlow {
			get => (ConnectorFlow)GetValue(ConnectorFlowProperty);
			set => SetValue(ConnectorFlowProperty, value);
		}

		public static readonly DependencyProperty ConnectorFlowProperty =
			DependencyProperty.Register("ConnectorFlow", typeof(ConnectorFlow), typeof(VisualNodeConnector), new PropertyMetadata(ConnectorFlow.Input));
		#endregion
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="programContext"></param>
		/// <param name="other"></param>
		/// <exception cref="VisualNodeLinkException"></exception>
		public void ConnectTo(VisualProgram programContext, VisualNodeConnector other) {
			// Perform a quick initial check to see if we maybe can connect the two connectors
			if (other == null
			 || NodeID == other.NodeID
			 || ConnectorFlow == other.ConnectorFlow) return;

			// Figure out which of the drag ends is the input/output of the connection
			var parentData = ConnectorFlow == ConnectorFlow.Input ? this : other;
			var childData = ConnectorFlow == ConnectorFlow.Output ? this : other;

			// Perform the link
			parentData.Node.Link(programContext, parentData.PropertyName, childData.Node);
			DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this)?.InvalidateVisual(); // Indicate that the canvas needs to be redrawn with a new connection line
		}
	}


	/// <summary>
	/// The data flow of a connector, either in or out.
	/// </summary>
	public enum ConnectorFlow {
		/// <summary>This connector represents a connection that expects an incoming parameter value or previous statement reference.</summary>
		Input,

		/// <summary>This connector represents a connection that expects an outgoing return value or statement flow.</summary>
		Output
	}
}
