using System.Windows;

namespace VisualProgrammer.WPF.Util {

	/// <summary>
	/// Dictionary that sets the <see cref="ResourceDictionary.Source"/> to be the given <see cref="SourceFile"/> converted to a pack URI.
	/// </summary>
	public class LocalResourceDictionary : ResourceDictionary {

		private static readonly string asmName = typeof(LocalResourceDictionary).Assembly.GetName().Name;

		private string sourceFile;

		/// <summary>
		/// The source file of this resource dictionary. Note this is always relative to the root of the VP WPF project.
		/// </summary>
		public string SourceFile {
			get => sourceFile;
			set {
				sourceFile = value;
				Source = new System.Uri($"pack://application:,,,/{asmName};component/{value}", System.UriKind.Absolute);
			}
		}
	}
}
