using System;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core {

	public sealed class Variable {

		private object? value;

		public Variable(string name, Type type) : this(name, type, type.IsValueType ? Activator.CreateInstance(type) : null) { }
		public Variable(string name, Type type, object? defaultValue) {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("The variable name must be non-null and non-whitespace.", nameof(name));
			Type = type ?? throw new ArgumentNullException(nameof(type), "The variable definition must have a type.");
			if (!Type.CanBeSetTo(defaultValue))
				throw new ArgumentException($"Value '{defaultValue?.ToString() ?? "null"}' cannot be set as the default value for this variable because it cannot be assigned to type '{Type.Name}'.", nameof(defaultValue));

			Name = name;
			value = DefaultValue = defaultValue;
		}

		public string Name { get; }
		public Type Type { get; }
		public object? DefaultValue { get; }

		public object? Value {
			get => value;
			set {
				if (!Type.CanBeSetTo(value))
					throw new ArgumentException($"Value of '{value?.ToString() ?? "null"}' cannot be assigned to variable of type '{Type.Name}'.", nameof(value));
				this.value = value;
			}
		}

		public void Reset() => value = DefaultValue;

		public Variable Clone() => new Variable(Name, Type, DefaultValue) { Value = Value };
	}
}
