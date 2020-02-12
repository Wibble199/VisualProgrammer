# Generic Nodes

VisualProgrammer supports the ability to use generic Nodes. If a generic node is added to the environment, it will appear with one or more dropdown boxes underneath in the toolbox. This allows the user to select from a dropdown which type or types they want.

> [!NOTE]
> Currently there is no way of limiting which types can be choosen by the user - I.E. any generic constraints are ignored when presenting the dropdown lists under the node in the toolbox. This is a planned feature and will hopefully be available soon.

To create a generic node, simply add one or more generic type arguments to the class as you would any other .NET class. This generic argument can be used in expression references, static value properties, variable references or even passed to the base `VisualExpression<T>` class.

To see an example, lets look at the implementation for the _Literal_ node, which allows users to add an unchanging constant value to the program.

```cs
public sealed class Literal<TValue> : VisualExpression<TValue> {

	[VisualNodeProperty] public TValue Value { get; set; }

	public override Expression CreateExpression(VisualProgram context) =>
		Expression.Constant(Value, typeof(TValue));
}
```

This is a generic node which takes a single type parameter which determines the type of value that it can edit. It extends a VisualExpression which returns the type selected by the user. It also has a static value property of the same type. If the user chose "String" as the TValue parameter, this would mean that the resulting node would return a string and also present a string editor on the node itself.

Similarly, the _SetVariable_ node is also a generic node, where the user can select the type of variable they want to set. It then requires a variable reference of this type and an expression of this type. For example if the user chose "Double", then they would be asked to provide an expression that returns a double and a double variable from the dropdown.

```cs
public sealed class SetVariable<TVar> : VisualStatement {

	[VisualNodeProperty] public VariableReference<TVar> Variable { get; set; }
	[VisualNodeProperty] public ExpressionReference<TVar> Value { get; set; }

	public override Expression CreateExpression(VisualProgram context) =>
		VariableAccessorFactory.CreateSetterExpression(context, Variable, Value.ResolveRequiredExpression(context));
}
```
