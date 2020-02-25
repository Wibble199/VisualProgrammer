# Variables

Variables are ways of storing stateful data to allow it to be reused again later.

Unlike many real programming languages, VisualProgrammer exclusively uses per-VisualProgram "global" variables. This means it is not possible to scope a variable to just one VisualEntry.

When the program is [compiled](compilation.md), these variables are stored inside the program instance. Each variable can be given a default value which it will have when the program is first initialised or when the ResetVariables method is called.

---
### Reasoning for a global variable system

One of the main reasons this was done was to simplify the compilation and UI logic. It means that there is a single place where all variables are defined, rather than having to evaluate the scope that a node is in and whether it would have access to a particular variable when validating the program. There would also be many strange cases to consider when, for example, two different flows combine (the system would have to check that it is valid on both branches), or when duplicating code, etc.

The second main reason a global system was implemented was because, in some use cases where the users may not be familar with the concepts of programming or scope, a user may not be aware that two local variables of the same name and type, but each under different entries are NOT referencing the same variable. By implementing a global variable system, the user knows for certain that every time a variable with a certain name is referenced, it always is refering to the same variable.