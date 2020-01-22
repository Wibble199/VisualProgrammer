using System.Windows;
using System.Windows.Input;

namespace VisualProgrammer.WPF.Util {

	public static class Commands {
		
		public static readonly RoutedCommand StartMove = new RoutedCommand("StartMove", typeof(Commands));


		/// <summary>Invokes the command on the target <see cref="ICommandSource"/>.</summary>
		internal static void InvokeCommand<T>(this T self) where T : ICommandSource, IInputElement {
			var cmd = self.Command;
			var trg = self.CommandTarget ?? self;
			var param = self.CommandParameter;
			if (cmd is RoutedCommand rc && rc.CanExecute(param, trg))
				rc.Execute(param, trg);
			else if (cmd?.CanExecute(param) == true)
				cmd.Execute(param);
		}
	}
}
