using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF {

    public class VisualNodeCanvas : ItemsControl {

        static VisualNodeCanvas() {
            // Indicate that we need to use our custom style (which is based off the default ItemsControl style)
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodeCanvas), new FrameworkPropertyMetadata(typeof(VisualNodeCanvas)));
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            // Draw lines between connectors
            if (Nodes != null)
                // Temporarily testing line drawing method
                foreach (var temp in Nodes)
                    drawingContext.DrawLine(new Pen(Brushes.Red, 2d), new Point(temp.Value.Position.X, temp.Value.Position.Y), new Point(0, 0));
        }

        #region Nodes DependencyProperty
        /// <summary>
        /// The nodes collection of a <see cref="VisualProgram"/>.
        /// </summary>
        public Dictionary<Guid, VisualNode> Nodes {
            get => (Dictionary<Guid, VisualNode>)GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public static readonly DependencyProperty NodesProperty =
            DependencyProperty.Register("Nodes", typeof(Dictionary<Guid, VisualNode>), typeof(VisualNodeCanvas), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
    }
}
