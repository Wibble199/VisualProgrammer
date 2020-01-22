using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisualProgrammer.WPF.Util;
using VisualProgrammer.WPF.ViewModels;

namespace VisualProgrammer.WPF {

	public class VisualNodePresenter : Control, ICommandSource {

		private const string PART_NodeDragArea = nameof(PART_NodeDragArea);

		static VisualNodePresenter() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodePresenter), new FrameworkPropertyMetadata(typeof(VisualNodePresenter)));
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			if (GetTemplateChild(PART_NodeDragArea) is UIElement r)
				r.MouseDown += (sender, e) => this.InvokeCommand();
		}

		#region ICommandSource
		#region Command DependencyProperty
		public ICommand Command {
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register("Command", typeof(ICommand), typeof(VisualNodePresenter), new PropertyMetadata(null));
		#endregion

		#region CommandParameter DependencyProperty
		public object CommandParameter {
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.Register("CommandParameter", typeof(object), typeof(VisualNodePresenter), new PropertyMetadata(null));
		#endregion

		#region CommandTarget DependencyProperty
		public IInputElement CommandTarget {
			get => (IInputElement)GetValue(CommandTargetProperty);
			set => SetValue(CommandTargetProperty, value);
		}

		public static readonly DependencyProperty CommandTargetProperty =
			DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(VisualNodePresenter), new PropertyMetadata(null));
		#endregion
		#endregion
	}

	/// <summary>
	/// Drag behaviour for moving a node presenter.
	/// </summary>
	internal class VisualNodePresenterDragBehaviour : ICanvasDragBehaviour {

		private VisualNodeViewModel nodeViewModel;
		private Point nodeRelativePoint;

		public VisualNodePresenterDragBehaviour(VisualNodeViewModel nodeViewModel, Point nodeRelativePoint) {
			this.nodeViewModel = nodeViewModel;
			this.nodeRelativePoint = nodeRelativePoint;
		}

		public bool ShouldCapture => true;

		public bool MoveStep(MouseEventArgs e, Point canvasPoint) {
			nodeViewModel.Position = new System.Drawing.Point((int)(canvasPoint.X - nodeRelativePoint.X), (int)(canvasPoint.Y - nodeRelativePoint.Y));
			return true;
		}

		public bool StopMove(MouseEventArgs e, Point canvasPoint) => false;
	}
}
