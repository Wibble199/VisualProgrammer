using System;
using System.Collections.Generic;
using System.Windows;
using VisualProgrammer.Core;
using VisualProgrammer.Core.Flow;
using VisualProgrammer.Core.Nodes.Variables;

namespace DemoWPFApp {

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowModel();
        }

		private void Button_Click(object sender, RoutedEventArgs e) {
			var fac = ((MainWindowModel)DataContext).Program.Compile();
			var prog = fac.CreateProgram();
			((dynamic)prog).DemoEntry();
		}
	}

	public class MainWindowModel {

        public VisualProgram Program { get; set; }

        public MainWindowModel() {
            var printGuid = Guid.NewGuid();
            var stringLitGuid = Guid.NewGuid();

            Program = new VisualProgram {
                EntryDefinitions = new Dictionary<string, EntryDefinition> {
                    ["demoEntry"] = new EntryDefinition { Name = "Demo Entry" }
                },
                Nodes = new Dictionary<Guid, VisualNode> {
                    [Guid.NewGuid()] = new VisualEntry("demoEntry") { FirstStatement = new StatementReference(printGuid), Position = new System.Drawing.Point(250, 0) },
                    [printGuid] = new Trace { PrintValue = new ExpressionReference<string>(stringLitGuid), Position = new System.Drawing.Point(250, 40) },
                    [stringLitGuid] = new Literal<string> { Value = "Hello World!", Position = new System.Drawing.Point(5, 100) },
					//[Guid.NewGuid()] = new SetVariable<string> { Variable = new VariableReference<string>("SomeString"), Position = new System.Drawing.Point(250, 150) }
					[Guid.NewGuid()] = new Trace { Position = new System.Drawing.Point(250, 150) },
					[Guid.NewGuid()] = new If { Position = new System.Drawing.Point(0, 250) }
				}
            };
        }
    }

	// Since Console.WriteLine doesn't seem to actually output anything, this is a similar statement that uses Trace.WriteLine instead
	public class Trace : VisualStatement {
		[VisualNodeProperty] public ExpressionReference<string> PrintValue { get; set; }

		private static readonly System.Reflection.MethodInfo write = typeof(System.Diagnostics.Trace).GetMethod("WriteLine", new[] { typeof(object) })!;

		public override System.Linq.Expressions.Expression CreateExpression(VisualProgram context)
			=> System.Linq.Expressions.Expression.Call(null, write, PrintValue.ResolveRequiredExpression(context));
	}
}
