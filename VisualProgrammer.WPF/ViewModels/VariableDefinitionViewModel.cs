using System;
using System.Linq;
using System.Windows.Input;
using VisualProgrammer.Core;
using VisualProgrammer.Core.Environment;

namespace VisualProgrammer.WPF.ViewModels {

	public sealed class VariableDefinitionViewModel : ViewModelBase<Variable> {

		private readonly VisualProgramEnvironment environment;
		internal event EventHandler RequestDelete;

		public VariableDefinitionViewModel(Variable model, VisualProgramEnvironment environment) : base(model) {
			this.environment = environment;
			DeleteCommand = new DelegateCommand(Delete, () => !IsLocked);
		}

		public string ID => model.Name;

		public Type Type => model.Type;

		public object Default => model.DefaultValue;

		public bool IsLocked => environment.LockedVariables.Any(v => v.Name.Equals(ID, StringComparison.OrdinalIgnoreCase));

		public ICommand DeleteCommand { get; }

		private void Delete() => RequestDelete?.Invoke(this, EventArgs.Empty);
	}
}
