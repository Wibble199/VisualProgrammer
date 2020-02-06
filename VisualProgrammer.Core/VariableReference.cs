using System;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Represents a reference to a variable in the program.
	/// </summary>
	/// <typeparam name="TVar">The type of variable accepted by this reference.</typeparam>
	public struct VariableReference<TVar> : IVariableReference {
		public string Name { get; }
		public Type Type => typeof(TVar);

		public VariableReference(string name) {
			Name = name;
		}

		public void Validate(VisualProgram context) {
			if (string.IsNullOrWhiteSpace(Name))
				throw new Exception(); // TODO: Add meaningful error message. If this is thrown, variable isn't provided
			if (!context.VariableDefinitions.TryGetValue(Name, out var def))
				throw new Exception(); // TODO: Add meaninful error message. If this is thrown, could not find a variable with the target name.
			if (def.Type != Type)
				throw new Exception(); // TODO: Add meaningful message. If this is thrown, variable is wrong type
		}

		public override bool Equals(object obj) => Name == (obj as VariableReference<TVar>?)?.Name;
		public static bool operator ==(VariableReference<TVar> a, VariableReference<TVar> b) => a.Equals(b);
		public static bool operator !=(VariableReference<TVar> a, VariableReference<TVar> b) => !a.Equals(b);
		public override int GetHashCode() => Name.GetHashCode();
	}

	/// <summary>Contains helper methods for <see cref="VariableReference{TVar}"/>.</summary>
	public static class VariableReference {
		/// <summary>Constructs a new <see cref="VariableReference{TVar}"/> of the target type with the given variable name.<para/>
		/// Does not validate that the variable name is correct and of the correct type.</summary>
		/// <param name="variableType">The type of variable being referenced.</param>
		/// <param name="varName">The name of the variable to be referenced.</param>
		public static IVariableReference Create(Type variableType, string varName) =>
			(IVariableReference)Activator.CreateInstance(typeof(VariableReference<>).MakeGenericType(variableType), varName);
	}

	public interface IVariableReference {
		string Name { get; }
		Type Type { get; }

		/// <summary>
		/// Validates the variable reference and throws an exception if it is invalid.
		/// If the reference is empty or null (i.e. not pointing to a variable), this will not throw an error.
		/// </summary>
		/// <param name="context">The context to validate the reference in.</param>
		void Validate(VisualProgram context);
	}
}
