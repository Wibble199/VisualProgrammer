using System;
using System.Collections.Generic;
using System.Windows;
using VisualProgrammer.Core;
using VisualProgrammer.Core.Nodes.Debug;
using VisualProgrammer.Core.Nodes.Variables;

namespace DemoWPFApp {

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowModel();
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
                    [printGuid] = new Print { PrintValue = new ExpressionReference<string>(stringLitGuid), Position = new System.Drawing.Point(250, 100) },
                    [stringLitGuid] = new Literal<string> { Value = "Hello World!", Position = new System.Drawing.Point(5, 100) }
                }
            };
        }
    }
}