using System;
using System.Windows.Markup;

namespace VisualProgrammer.WPF.Util {

	/// <summary>
	/// Helper extension that can be used in XAML markup to instantiate a generic object of the target type.
	/// </summary>
	public class Generic : MarkupExtension {

		private object instance;

		/// <summary>
		/// The base (generic) type that will be instantiated.
		/// </summary>
		public Type BaseType { get; set; }

		/// <summary>
		/// The generic type arguments that will be passed to the 
		/// </summary>
		public Type[] TypeArguments { get; set; }

		/// <summary>
		/// Sets a single type argument.
		/// </summary>
		/// <remarks>Easier for a single type than adding a xaml array.</remarks>
		public Type TypeArgument { set => TypeArguments = new[] { value }; }

		/// <summary>
		/// Provides the instance of the generic type.
		/// </summary>
		public override object ProvideValue(IServiceProvider serviceProvider) {
			if (instance == null) {
				var concreteType = BaseType.MakeGenericType(TypeArguments);
				instance = Activator.CreateInstance(concreteType);
			}
			return instance;
		}
	}
}
