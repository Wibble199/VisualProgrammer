using System;
using System.Diagnostics;
using System.Windows;
using VisualProgrammer.Core;

namespace DemoWPFApp {

	public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowModel();
        }

		private void Button_Click(object sender, RoutedEventArgs e) {
			var fac = ((MainWindowModel)DataContext).Program.Compile<IDemoProgram>();
			var prog = fac.CreateProgram();
			prog.SomeNewLockedDouble = 10d;
			prog.DemoEntry();
			Trace.WriteLine($"SomeNewLockedDouble = {prog.SomeNewLockedDouble}");
		}
	}

	public interface IDemoProgram {
		void DemoEntry();
		double SomeNewLockedDouble { get; set; }
	}

	public class MainWindowModel {

        public VisualProgram Program { get; set; }

		public MainWindowModel() {
			Program = new VisualProgram(env => env
				.ConfigureNodes(n => n
					.IncludeDefault()
					.Exclude<VisualProgrammer.Core.Nodes.Debug.Print>()
					.Include<TraceStmt>()
				)
				.ConfigureEntries(e => e
					.Add("demoEntry", "Demo Entry")
					.Add("demoEntry2", "Demo Entry 2")
				).ConfigureLockedVariables(v => v
					.Add<int>("someIntVar", 20)
					.Add<double>("somenewLockedDouble")
				),
				new VisualNodeCollection(),
				new VariableCollection {
					{ "someStringVar", typeof(string), "Hello from the someStringVar!" },
					{ "otherStrVar", typeof(string), "Greetings from the otherStrVar!" },
					{ "someIntVar", typeof(int), 10 }
				}
			);
        }
    }

	// Since Console.WriteLine doesn't seem to actually output anything, this is a similar statement that uses Trace.WriteLine instead
	public class TraceStmt : VisualStatement {
		[VisualNodeProperty] public ExpressionReference<string> PrintValue { get; set; }

		private static readonly System.Reflection.MethodInfo write = typeof(Trace).GetMethod("WriteLine", new[] { typeof(object) })!;

		public override System.Linq.Expressions.Expression CreateExpression(VisualProgram context)
			=> System.Linq.Expressions.Expression.Call(null, write, PrintValue.ResolveRequiredExpression(context));
	}
}
