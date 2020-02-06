using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.ViewModels;

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

	public class ProgramViewModelConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is VisualProgram vp ? new VisualProgramViewModel(vp) : null;
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
