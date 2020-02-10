using System;
using System.Collections.Generic;

namespace VisualProgrammer.Core.Environment {

	/// <summary>
	/// A fluent API for setting the locked variables in the environment. These variables cannot be removed, renamed or have their types changed.
	/// </summary>
	public sealed class LockedVariableConfigurator {

		internal readonly Dictionary<string, Variable> variables = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);

		internal LockedVariableConfigurator() { }

		private LockedVariableConfigurator Add(Variable @var) {
			if (variables.ContainsKey(var.Name))
				throw new ArgumentException($"Cannot lock a variable with name '{var.Name}'. A variable by that name already exists.", nameof(var));
			variables.Add(var.Name, var);
			return this;
		}

		/// <summary>
		/// Creates a new locked variable with the given name, type and default value.
		/// </summary>
		public LockedVariableConfigurator Add(string name, Type type, object? @default) => Add(new Variable(name, type, @default));

		/// <summary>
		/// Creates a new locked variable with the given name, type and default value.
		/// </summary>
		public LockedVariableConfigurator Add<TVar>(string name, TVar @default) => Add(name, typeof(TVar), @default);

		/// <summary>
		/// Creates a new locked variable with the given name and type.
		/// The default value of this variable is automatically determined from the type.
		/// </summary>
		public LockedVariableConfigurator Add(string name, Type type) => Add(new Variable(name, type));

		/// <summary>
		/// Creates a new locked variable with the given name and type.
		/// The default value of this variable is automatically determined from the type.
		/// </summary>
		public LockedVariableConfigurator Add<TVar>(string name) => Add(name, typeof(TVar), default(TVar));
	}
}
