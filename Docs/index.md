# **VisualProgrammer**

VisualProgrammer is a tool that allows users of your application to build a program visually out of blocks and connect them to pass data/control flow. These programs can then be compiled using Linq Expressions, meaning that once compiled the program performs at speeds similar to the speed you would get if you'd written and compiled the program with C#.

The core of VisualProgrammer provides the base features that make up the main functionality and then there are other projects that implement the drag-drop user interface in different frameworks. Currently WPF and Blazor are supported.

The goals of this project are to provide a programming interface that:
- Is easily interoperable with other applications - producing simple `System.Action`s which can be called whenever and where ever they are needed.
- Able to be used by people of all experience levels - simple enough to be easily picked up, powerful enough to do do almost anything.
- Is easily extendable - allowing easy adding and registering of new nodes that can better fit another project's needs.