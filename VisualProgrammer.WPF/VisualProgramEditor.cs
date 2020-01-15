using System.Windows;
using System.Windows.Controls;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF {

    public class VisualProgramEditor : Control {

        static VisualProgramEditor() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualProgramEditor), new FrameworkPropertyMetadata(typeof(VisualProgramEditor)));
        }

        #region Program DependencyProperty
        public VisualProgram Program {
            get => (VisualProgram)GetValue(ProgramProperty);
            set => SetValue(ProgramProperty, value);
        }

        public static readonly DependencyProperty ProgramProperty =
            DependencyProperty.Register("Program", typeof(VisualProgram), typeof(VisualProgramEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
    }
}
