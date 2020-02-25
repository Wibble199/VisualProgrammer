# Compilation

When a VisualProgram is compiled using the Compile methods, a program factory class is created which is responsible for creating the self-contained program instances. The program that has been created by the user will be compiled to IL code through the user of Linq expressions. During compilation, a new dynamic class is created which extends or inherits a specified class/interface (called `TExtends`), as well as implementing [`ICompiledInstanceBase`](xref:VisualProgrammer.Core.Compilation.ICompiledInstanceBase).

When the dynamic type is created, VisualProgrammer will attempt to bind the user's compiled functions and the variables to methods and properties on `TExtends` and any of it's superclasses or interfaces:
- For each entry defined in the program's environment, a method of the same name as the entry (case insensitive) will be created if:
	- `TExtends` is a class and a `public abstract` or `protected abstract` method of that name exists on `TExtends` or any of its superclasses.
	- `TExtends` is a class and a `public virtual` or `protected abstract` method of that name exists on `TExtends` or any of its superclasses, but **only** if the compiled delegate is not empty. So, if the user does not add that entry to the canvas or adds it but does not connect any statements to it, the virtual method will not be overriden. This allows you to provide "fallback" logic.
	- Any of `TExtends`'s interfaces (including `TExtends` itself if it is an interface) defines a method of that name.
- For each variable that exists in the program, a property of the same name (case insensitive) and read-write status is created on the dynamic type if:
	- `TExtends` or any of it's superclasses define a `public virtual` or `protected virtual` property of that name.
	- Any of `TExtends`'s interfaces(including `TExtends` itself if it is an interface) defines a property of that name.
- If `TExtends` is a class and there are any constructors defined, these will also be implemented on the created type and will pass any parameters tat are given to the `CreateProgram` call onto the constructor.
	- One very important caveat with constructors however, is that they are unable to access the variables defined in the program (including properties that are bound to these variables) because the variable store has not been initialised yet.

Note that all methods must be of a void type and must have parameters that match the ones defined on the entry definition.

All variables - regardless of whether they are locked or bound to a property - can be read or written to using the `ICompiledInstanceBase`'s `SetVariable` and `GetVariable` methods. Variable accessors are not automatically created on the dynamic type to prevent a name collision between variables and entries.

## Examples
That's quite a lot to understand, but here are some examples using the following program definition. If you're not sure, it's easiest to start with an interface.

```cs
var program = new VisualProgram(env => env
	.ConfigureEntries(e => e
		.Add("paramEntry", "Parameterized Entry", p => p.WithParameter<double>("someDouble"))
		.Add("nonParamEntry", "No Parameters")
	),
	new VisualNodeCollection {
		...
	},
	new VariableCollection {
		{ "doubleVar", typeof(double), 0d },
		{ "boolVar", typeof(bool), false }
	}
);
```


### Interface
```cs
// Compile the program and create an instance
var factory = program.Compile<IMyProgram>();
var instance = factory.CreateProgram();

// We can now use this instance as a normal object
instance.ParamEntry(10d);
instance.DoubleVar = 1.23d;
instance.NonParamEntry();
Console.WriteLine(instance.DoubleVar);


// Our interface for the typed program
// It does not have to extend from ICompiledInstanceBase, but if it does it allows us to use ResetVariables, GetVariable and SetVariable
interface IMyProgram : ICompiledInstanceBase {
	void ParamEntry(double d);
	void NonParamEntry();
	double DoubleVar { get; set; }
}
```

### Simple Abstract Class
```cs
public abstract class SimpleAbstract : ParentClass {

	public abstract void ParamEntry(double d);
	public double DoubleVar { get; set; }

	public void AdditionalStuffOnTheClass() {
		// You can add your own methods that don't relate to an entry which will be inherited.
	}
}

// Inheritance works, so we can define the entry on other superclasses
public abstract class ParentClass {
	// Make sure the method is virtual if not abstract (if it relates to an entry) so it can be overriden by the VisualProgrammer compiler
	public virtual void NonParamEntry() { }
}
```

### Complex Abstract Class
```cs
// The class must be public so it can be seen by the compiler
// Once again, it does not have to specify it implements ICompiledBase if you do not need to use the variable accessor methods
public abstract class AbstractProgram : ICompiledInstanceBase {

	public AbstractProgram(string name) {
		Console.WriteLine("Printed from the constructor. Hello " + name);
	}
	
	// This works. A property that can be get publicly, but only set privately
	public virtual double DoubleVar { get; protected set; }

	public abstract void NonParamEntry();

	protected virtual void ParamEntry(double d) {
		// This will only be executed if the ParamEntry is not given any statements to execute
		Console.WriteLine("You added nothing to the program");
	}

	// This will never be overwritten even if an entry existed with the same name, because it's not virtual or abstract
	public void ExecuteMyProtectedMethod() {
		ParamEntry(3.14d); // Obviously you can access protected methods here
	}

	// These methods are required to make the class implement ICompiledInstanceBase. They will be implemented by the VisualProgram compiler.
	public abstract void ResetVariables();
	public abstract object GetVariable(string key);
	public abstract void SetVariable(string key, object value);
}


// Compile the program into a factory and create it. We must pass an argument for the AbstractProgram constructor
var factory = progam.Compile<AbstractProgram>();
instance = factory.CreateProgram("World!"); // Prints "Printed from the constructor. Hello World!"
// instance = factory.CreateProgram(); // Throws an error since there is no parameterless constructor
```