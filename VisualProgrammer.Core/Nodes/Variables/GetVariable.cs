using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Variables {

    public sealed class GetVariable<TVar> : VisualExpression<TVar> {

		[VisualNodeValueProperty]
		public string VariableName { get; set; } = "";

        public override Expression CreateExpression(VisualProgram context) {
            // Check that the variable exists in the program and it has the correct type
            // Note that the check is if TVar is assignable from the variable type's. That means that if TVar is less specific from the variable type, e.g.
            // the variable is an int but the TVar is an object, then we can still get the value.
            if (!context.VariableDefinitions.TryGetValue(VariableName, out var def) || !typeof(TVar).IsAssignableFrom(def.type))
                throw new System.Exception(""); // TODO: Add a more meaningful message

            return VariableAccessorFactory.CreateGetterExpression(context, VariableName, typeof(TVar));
        }
    }
}
