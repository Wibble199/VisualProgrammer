using System.Windows;
using System.Windows.Controls;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF {

    public class VisualNodePresenter : Control {

        static VisualNodePresenter() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodePresenter), new FrameworkPropertyMetadata(typeof(VisualNodePresenter)));
        }

        #region VisualNode DependencyProperty
        public VisualNode Node {
            get => (VisualNode)GetValue(NodeProperty);
            set => SetValue(NodeProperty, value);
        }

        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register("Node", typeof(VisualNode), typeof(VisualNodePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public DataTemplate NodeTemplate => TryFindResource(Node.GetType()) as DataTemplate;
    }
}
