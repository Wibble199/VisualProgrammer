using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF {

    public class VisualNodeCanvas : ItemsControl {

		// Holds data that can uniquely identify the connector that is currently being dragged.
		private ConnectorData dragData;

        static VisualNodeCanvas() {
            // Indicate that we need to use our custom style (which is based off the default ItemsControl style)
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodeCanvas), new FrameworkPropertyMetadata(typeof(VisualNodeCanvas)));
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            // Draw lines between connectors
            if (Program?.Nodes != null)
                // Temporarily testing line drawing method
                foreach (var temp in Program.Nodes)
                    drawingContext.DrawLine(new Pen(Brushes.Red, 2d), new Point(temp.Value.Position.X, temp.Value.Position.Y), new Point(0, 0));
        }

		internal void StartDrag(Guid nodeId, object p, string v1, bool v2) {
			throw new NotImplementedException();
		}

		#region Program DependencyProperty
		public VisualProgram Program {
            get => (VisualProgram)GetValue(ProgramProperty);
            set => SetValue(ProgramProperty, value);
        }

        public static readonly DependencyProperty ProgramProperty =
            DependencyProperty.Register("Program", typeof(VisualProgram), typeof(VisualNodeCanvas), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		/// <summary>
		/// Indicates to the canvas that a drag operation (originating from a node connector) has begun.
		/// </summary>
		/// <param name="dragData">The data of the connector that the drag operation started on.</param>
		internal void StartDrag(ConnectorData dragData) {
			this.dragData = dragData;
		}

		/// <summary>
		/// Indicates to the canvas that a drag operation has finished (terminating on a node connector).
		/// Creates a link if valid.
		/// </summary>
		/// <param name="dropData">The data of the connector that the drop happened on.</param>
		internal void EndDrag(ConnectorData dropData) {
			// Check the types match, the `isInput`s do not and the IDs are not the same
			if (dragData == null
			 || dragData.nodeId == dropData.nodeId
			 || dragData.isInput == dropData.isInput
			 || dragData.type != dropData.type) return;

			// Figure out which of the drag ends is the input/output of the connection
			var parentData = dragData.isInput ? dragData : dropData;
			var childData = !dragData.isInput ? dragData : dropData;

			// Perform the link
			try {
				parentData.node.Link(Program, parentData.name, childData.node);
				InvalidateVisual(); // Indicate that the canvas needs to be redrawn with the new line
			} catch (VisualNodeLinkException ex) {
				// TODO: Add user feedback
			}
		}
	}


	/// <summary>
	/// Simple object that stores data able to indentify a connector.
	/// </summary>
	internal class ConnectorData {
		internal Guid nodeId;
		internal VisualNode node;
		internal VisualNodePropertyType type;
		internal string name;
		internal bool isInput;
	}
}
