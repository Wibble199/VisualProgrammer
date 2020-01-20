using System;
using System.Windows.Input;

namespace VisualProgrammer.WPF.ViewModels {

	public sealed class DelegateCommand : ICommand {

		private readonly Action execute;
		private readonly Func<bool> canExecute;

		public DelegateCommand(Action execute, Func<bool> canExecute = null) {
			this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter) => canExecute?.Invoke() ?? true;
		public void Execute(object parameter) => execute();
	}

	public sealed class DelegateCommand<TParameter> : ICommand {

		private readonly Action<TParameter> execute;
		private readonly Predicate<TParameter> canExecute;

		public DelegateCommand(Action<TParameter> execute, Predicate<TParameter> canExecute = null) {
			this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter) => canExecute?.Invoke((TParameter)parameter) ?? true;
		public void Execute(object parameter) => execute((TParameter)parameter);
	}
}
