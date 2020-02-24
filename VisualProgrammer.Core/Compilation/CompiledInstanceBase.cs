namespace VisualProgrammer.Core.Compilation {

	/// <summary>
	/// An interface that is implemented by the <see cref="CompiledInstanceBase"/>.
	/// Custom program interfaces can extend this interface to provide strict-typing for getting or setting non-property-bound variables.
	/// </summary>
	public interface ICompiledInstanceBase {
		/// <summary>
		/// Resets all variables to their default values as defined by the given variable definitions dictionary.
		/// </summary>
		void ResetVariables();

		/// <summary>
		/// Attempts to get the value of the variable with the given name.
		/// </summary>
		/// <param name="key">The name/key of the variable. Case-insensitive.</param>
		object? GetVariable(string key);

		/// <summary>
		/// Attempts to set the value of the variable with the given name to the given value.
		/// </summary>
		/// <param name="key">The name/key of the variable. Case-insensitive.</param>
		void SetVariable(string key, object? value);
	}
}
