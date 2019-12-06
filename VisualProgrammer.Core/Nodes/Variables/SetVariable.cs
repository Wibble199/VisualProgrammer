using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Variables {

    public sealed class SetVariable<TVar> : VisualStatement {

		[VisualNodeValueProperty]
		public string VariableName { get; set; } = "";

        [VisualNodeExpressionProperty(nameof(TVar))]
        public NodeReference? Value { get; set; }

        public override Expression CreateExpression(VisualProgram context)
        {
            // Check that the variable exists in the program and it has the correct type
            if (!context.VariableDefinitions.TryGetValue(VariableName, out var def) || !def.type.IsAssignableFrom(typeof(TVar)))
                throw new System.Exception(""); // TODO: Add a more meaningful message

            return VariableAccessorFactory.CreateSetterExpression(context, VariableName, Value.ResolveRequiredExpression(context));
        }
    }
}
