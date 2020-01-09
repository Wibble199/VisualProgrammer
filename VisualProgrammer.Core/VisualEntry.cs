using VisualProgrammer.Core.Nodes.Variables;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VisualProgrammer.Core.Utils;
using System;

namespace VisualProgrammer.Core {

    public sealed class VisualEntry : VisualNode {

		public VisualEntry(string visualEntryId) {
			VisualEntryId = visualEntryId;
		}

		// The ID of the definition of this entry in the VisualProgram's dictionary
		public string VisualEntryId { get; set; }

        /// <summary>A map that contains the incoming entry parameter name onto a program variable.
        /// The key of the map represents the parameter name (as defined in the relevant VisualEntryDefinition), and the value is the name of a program variable.</summary>
        public Dictionary<string, string> ParameterMap { get; set; } = new Dictionary<string, string>();

        /// <summary>A reference to the first statement that will be executed by this entry.</summary>
        [VisualNodeProperty(Label = "Start")] public StatementReference FirstStatement { get; set; }

        /// <summary>
        /// Creates the lambda expression from the statements attached to this entry and the parameter definitions in the provided program context.
        /// </summary>
        public LambdaExpression CreateLambda(VisualProgram context) {
            if (!context.EntryDefinitions.TryGetValue(VisualEntryId, out var def))
                throw new KeyNotFoundException($"Could not find an entry with ID '{VisualEntryId}' in the VisualProgram's entry definitions.");

            // Create a parameter for each expected/defined parameter
            var parameters = def.Parameters.Map(Expression.Parameter);

            return Expression.Lambda(
                Expression.Block(
                    // Before doing the main body of the entry function, make expressions to copy the incoming parameters to variables
                    ParameterMap
                        .Where(mapping => !string.IsNullOrWhiteSpace(mapping.Value)) // Exclude any that don't actually map to anything
                        .Select(p => VariableAccessorFactory.CreateSetterExpression(
                            context,
                            (IVariableReference)Activator.CreateInstance(typeof(VariableReference<>).MakeGenericType(def.Parameters[p.Key]), p.Value),
                            parameters[p.Key]
                        )
                    ).Concat(
                        // Then do the actual main body
                        NodeUtils.FlattenExpressions(context, FirstStatement)
                    )
                ),

				// All lambdas will get a context parameter
				// We also pass the defined parameters to the lambda so it knows what types to expect and what signature to have.
				// This needs to happen regardless or not of whether the parameters are actually used by the entry function as this determines the delegate's signature.
				new[] { context.compiledInstanceParameter }.Concat(parameters)
            );
        }


        /// <summary>Alias for <see cref="CreateLambda(VisualProgram)"/>.</summary>
        public override Expression CreateExpression(VisualProgram context) => CreateLambda(context);
    }
}
