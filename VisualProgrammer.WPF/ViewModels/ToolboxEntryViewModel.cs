using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF.ViewModels {

	public class ToolboxEntryViewModel : ViewModelBase<EntryDefinition> {

		private readonly VisualProgramViewModel parent;

		internal ToolboxEntryViewModel(VisualProgramViewModel parent, EntryDefinition model) : base(model) {
			this.parent = parent;
			parent.PropertyChanged += Parent_PropertyChanged;
		}

		public bool CanAdd => !parent.model.Nodes.OfType<VisualEntry>().Any(ve => ve.VisualEntryId == model.Id);

		public VisualNodeViewModel TemplateInstance => new VisualNodeViewModel(new VisualEntry(model.Id));

		public void CreateNode(object sender, MouseEventArgs e) {
			if (CanAdd) {
				var nodeVm = parent.CreateNode(model);
				parent.DragBehaviour = new VisualNodePresenterDragBehaviour(nodeVm, e.GetPosition((IInputElement)sender));
			}
		}

		private void Parent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(VisualProgramViewModel.Nodes))
				Notify(nameof(CanAdd));
		}
	}
}
