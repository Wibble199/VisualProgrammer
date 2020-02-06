using System;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF.ViewModels {

	public sealed class VariableDefinitionViewModel : ViewModelBase<Variable> {

		public VariableDefinitionViewModel(Variable model) : base(model) { }

		public string ID => model.Name;

		public Type Type => model.Type;

		public object Default => model.DefaultValue;
	}
}
