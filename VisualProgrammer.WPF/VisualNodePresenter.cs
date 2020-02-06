using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisualProgrammer.WPF.Util;
using VisualProgrammer.WPF.ViewModels;

namespace VisualProgrammer.WPF {

	public class VisualNodePresenter : Control {

		private const string PART_NodeDragArea = nameof(PART_NodeDragArea);

		static VisualNodePresenter() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodePresenter), new FrameworkPropertyMetadata(typeof(VisualNodePresenter)));
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			if (GetTemplateChild(PART_NodeDragArea) is UIElement r)
				r.MouseDown += ExecuteDragCommand;
		}

		#region Command DependencyProperty
		public ICommand StartDragCommand {
			get => (ICommand)GetValue(StartDragCommandProperty);
			set => SetValue(StartDragCommandProperty, value);
		}

		public static readonly DependencyProperty StartDragCommandProperty =
			DependencyProperty.Register("StartDragCommand", typeof(ICommand), typeof(VisualNodePresenter), new PropertyMetadata(null));
		#endregion

		private void ExecuteDragCommand(object sender, MouseButtonEventArgs e) {
			var param = e.GetPosition(this);
			if (StartDragCommand is RoutedCommand rc)
				rc.Execute(param, this);
			else if (StartDragCommand is ICommand c)
				c.Execute(param);
		}
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
