﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.Util;

namespace VisualProgrammer.WPF {

	public class VisualNodeCanvas : ItemsControl {

		// Holds data that can uniquely identify the connector that is currently being dragged.
		private VisualNodeConnector dragSource;

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
			MouseUp += EndDrag;
		}

		protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

			// Draw lines between connectors
			if (Program?.Nodes != null) {
				foreach (var node in Program.Nodes) {
					foreach (var prop in node.Value.GetPropertiesOfType(VisualNodePropertyType.Expression, VisualNodePropertyType.Statement)) {
						// Check if the property has a reference to an expression, if so, find the connector points
						if (prop.Getter(node.Value) is INodeReference nr && nr.Id.HasValue) {
							var start = DependencyObjectUtils.ChildOfType<VisualNodeConnector>(this, c => c.NodeID == node.Key && c.PropertyName == prop.Name && c.ConnectorFlow == ConnectorFlow.Destination);
							var end = DependencyObjectUtils.ChildOfType<VisualNodeConnector>(this, c => c.NodeID == nr.Id.Value && c.ConnectorFlow == ConnectorFlow.Source);

							if (start != null && end != null)
								DrawLine(drawingContext, start, end, prop.PropertyType == VisualNodePropertyType.Statement);
						}
					}
				}
			}
        }

		/// <summary>
		/// Draws a connection line between the given connectors.
		/// </summary>
		/// <param name="isVertical">Whether the curve should be drawn as if it is connecting elements vertically (true) or horizontally (false).</param>
		private void DrawLine(DrawingContext ctx, VisualNodeConnector start, VisualNodeConnector end, bool isVertical) {
			// Start and end points of the connectors relative to the canvas
			var sp = start.TransformToAncestor(this).Transform(start.MidPoint);
			var ep = end.TransformToAncestor(this).Transform(end.MidPoint);

			// Bezier control points
			var cp1 = isVertical ? new Point(sp.X, (sp.Y + ep.Y) / 2) : new Point((sp.X + ep.X) / 2, sp.Y);
			var cp2 = isVertical ? new Point(ep.X, (sp.Y + ep.Y) / 2) : new Point((sp.X + ep.X) / 2, ep.Y);

			ctx.DrawGeometry(Brushes.Transparent, new Pen(Brushes.Red, 2d), new PathGeometry(new[] {
				new PathFigure(
					start.TransformToAncestor(this).Transform(start.MidPoint),
					new[] { new BezierSegment(cp1, cp2, ep, true) },
					false
				)
			}));
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
		/// <param name="dragSource">The connector that the drag operation started on.</param>
		internal void StartDrag(VisualNodeConnector dragSource) {
			this.dragSource = dragSource;
		}

		/// <summary>
		/// Indicates to the canvas that a drag operation has finished (terminating on a node connector).
		/// Creates a link if valid.
		/// </summary>
		/// <param name="dropSource">The connector that the drop happened on.</param>
		internal void EndDrag(VisualNodeConnector dropSource) {
			try {
				dragSource?.ConnectTo(Program, dropSource);

			} catch (VisualNodeLinkException ex) {
				// TODO: Add user feedback
			}
		}

		/// <summary>
		/// Indicates the drag has finished without any action.
		/// </summary>
		private void EndDrag(object sender, MouseEventArgs e) {
			dragSource = null;
		}
	}
}
