using System;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF.ViewModels {

	/// <summary>
	/// View-model for a single property that is part of a VisualNode. Note that the VisualNode context is also captured by this VM.
	/// </summary>
	public sealed class VisualNodePropertyViewModel : ViewModelBase<VisualNodePropertyDefinition> {

		private readonly VisualNode nodeContext;

		public VisualNodePropertyViewModel(VisualNodePropertyDefinition model, VisualNode nodeContext) : base(model) {
			this.nodeContext = nodeContext;
		}

		// Property forwarders for the normal (read-only) properties on the wrapped visual property definition
		public string Name => model.Name;
		public string DisplayName => model.DisplayName;
		public Type Type => model.PropertyDataType;
		public VisualNodePropertyType PropertyType => model.PropertyType;
		public Type RawType => model.RawType;
		public VisualNodePropertyAttribute Meta => model.Meta;

		/// <summary>
		/// Gets or sets the value of this property on the <see cref="VisualNode"/> captured by this view-model.
		/// </summary>
		public object Value {
			get => model.Getter(nodeContext);
			set {
				if (!Equals(model.Getter(nodeContext), value)) {
					model.Setter(nodeContext, value);
					Notify();
				}
			}
		}
	}
}
