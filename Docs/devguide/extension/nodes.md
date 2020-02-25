# Creating a Basic Custom Node Type

It is possible to create custom node types to integrate more deeply with whatever the program is being created for. For example, if you were making an IoT style program, you could add an expression node that checks whether a device is connected to the internet or perhaps add a statement that triggers a push notification.

To be able to add a node, you must be able to create a Linq expression for the action. Note however, it is possible to make a simple Linq expression that calls a method.

In this guide, we'll walk through creating a simple node that will add 10 to another value, then multiply it by 2 and return that value.

## 1. Creating the Class

To make a node, you first need to figure out what kind of node best suits the function.
Simply, if your node will be returning a value to be used by other nodes, you will extend `VisualExpression<T>` (where `T` is the type of data that the expression will return). If your node performs an action and should be able to be part of the program's flow, then you will want to extend `VisualStatement`.

For this guide, our node will be returning a value, so we want to make a VisualExpression. Create a new class for the node and extend the expression base class. Here's our first bit of code:

```cs
using VisualProgrammer.Core;

namespace CustomProgrammerNodes {
	public class AddTenThenDouble : VisualExpression<double> {
		// TODO
	}
}

```

We will also want to add our custom node to the environment so that the visual program editors allow the user to add our node to the program canvas. When creating the program, we can give it an action to configure the environment. Read more on the [environment](../core/environment.md) page.

```cs
new VisualProgram(env => env
	.ConfigureNodes(n => n
		.IncludeDefault()
		.Include<AddTenThenDouble>()
	...
```

Once we've finished implementing the abstract methods on our node, we can compile and run the program.

## 2. Initial Functionality

The next step is to implement the abstract `CreateExpression` method. This method takes a `VisualProgram` context and returns a `System.Linq.Expressions.Expression`. To create an expression, the [methods on the Expression class](https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression?view=netstandard-2.1#methods) are used. There are far too many to detail here, but some examples include `Constant` to get a static value, `AddChecked` to add two numbers together (and check for overflow errors), etc. Some of these expressions may in turn require other expressions to be passed in (this makes an Expression Tree).

At first, we'll just use a static value of three for now instead of a parameter - we'll get to parameters later, let's make our Linq tree first.

The first step is to take our input of three and then add ten to it. We will therefore need to use an `AddChecked` expression and pass it our two numbers as expressions:

```cs
Expression.AddChecked(
	Expression.Constant(3, typeof(double)), // We will replace this later
	Expression.Constant(10, typeof(double)) // Just add 10 to it
)
```

We then want to take this value that comes out of this expression, and multiply it by two. To do that we'll use `MultiplyChecked`.

I also like to add in `using static System.Linq.Expressions.Expression;` at the top of the file, so we don't have to prefix our methods with Expression.

```cs
MultiplyChecked(
	AddChecked(...),
	Constant(2, typeof(double))
)
```

What we've generated here is a Linq version of the expression `(3 + 10) * 2`. So, we just want to return this expression as our `CreateExpression` method.

The custom node will now look like this:

```cs
public class AddTenThenDouble : VisualExpression<double> {
	public override Expression CreateExpression(VisualProgram context) =>
		MultiplyChecked(
			AddChecked(
				Constant(3d, typeof(double)),
				Constant(10d, typeof(double))
			),
			Constant(2d, typeof(double))
		);
}
```
If we run our program, we should see our custom node in the toolbox of the editor. Dragging and dropping this node onto the canvas and connecting it to a print statement (you may need a ToString node too) should print out the value `26` in the console.

> [!NOTE]
> The `CreateExpression` method is called once when the program is compiled. This means that any conditions here (such as if statements) will execute at _compile_ time and NOT at _runtime_. Anything that needs to happen at runtime needs to be built-in to the expression returned by this method.

I would also recommend finding a Linq Expression Tree guide if you want to learn more about these expressions.

## 3. Adding Properties

The next thing we want to do is make it so our node is capable of taking a value as it's input instead of having a hard-coded value. To achieve this we can add properties to the node and mark it with the `VisualNodeProperty` attribute. There are a few types of properties that are supported by the editor:

# [Expression references](#tab/tabid-exprref)
Which the user can connect to another expression to source the value at runtime. This will likely be the most common property type. The type pased in as the generic argument is the type of expression that can be connected. It is possible to pass a type argument defined on the node into the reference type argument.
```cs
[VisualNodeProperty] public ExpressionReference<double> SomeDouble { get; set; }
```

# [Fixed values](#tab/tabid-fixedval)
These are entered by the user before the program is compiled. These can be used to conditionally generate the expression. For example, the default comparison expression uses an enum allowing the user to pick which operator to use.
```cs
[VisualNodeProperty] public ActionEnum Action { get; set; }
```

# [Variable references](#tab/tabid-varref)
Which point to a variable of a specific kind. Generally, it's better to use an expression reference and allow the user to link it to a "GetVariable" node if they want to read a variable. Like the expression references, the type passed to the VariableReference's type argument is the data type of the variable and it's possible to pass a generic argument defined on the node.
```cs
[VisualNodeProperty] public VariableReference<TVar> Variable { get; set; }
```

# [Statement references](#tab/tabid-stmtref)
Which you will likely never need to be used and can point to other statements. This is for nodes that may affect the control flow (such as loops or if conditionals) and will be touched on in a different tutorial.

By default, all VisualStatements inherit a `NextStatement` statement reference which determines the next statement to execute.

```cs
[VisualNodeProperty] public StatementReference AnotherBranch { get; set; }
```
***

For our node, we want users to be able to connect our node to another node that provides a value therefore we need to use an expression reference. We want a double type, so that will be the generic argument of the expression reference.

Next, we need to get an expression from this reference that we can pass to our Linq expression. This is simple, we just need to replace the `Constant(3d, typeof(double))` line with `Value.ResolveRequiredExpression(context)`. This will get the expression of the referenced expression when the user compiles the program. We used `ResolveRequiredExpression` because it throws an error if there is no expression connected. Alternatively you can use `ResolveExpression` which returns null if there is no connected expression, allowing you to use a different expression in it's place (e.g. a fallback default value).

Our class is now done. It takes an double expression and returns `(value + 10) * 2`. Run the program and try it out. The final code for the node is shown below.

```cs
using System.Linq.Expressions;
using VisualProgrammer.Core;

using static System.Linq.Expressions.Expression;

namespace CustomProgrammerNodes {
	public class AddTenThenDouble : VisualExpression<double> {

		[VisualNodeProperty] public ExpressionReference<double> Value { get; set; }

		public override Expression CreateExpression(VisualProgram context) =>
			MultiplyChecked(
				AddChecked(
					Value.ResolveExpression(context),
					Constant(10d, typeof(double))
				),
				Constant(2d, typeof(double))
			);
	}
}
```