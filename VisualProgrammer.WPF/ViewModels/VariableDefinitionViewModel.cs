using System;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF.ViewModels {

	public class VariableDefinitionViewModel : ViewModelBase<Variable> {

		public VariableDefinitionViewModel(Variable model, string id) : base(model) {
			ID = id;
		}

		public string ID { get; }

		public Type Type => model.Type;

		public object Default => model.DefaultValue;
	}
}
