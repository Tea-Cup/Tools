using System.Diagnostics.CodeAnalysis;

namespace ImageAOT {
	internal static class Program {
		[STAThread]
		static void Main(string[] args) {
			ApplicationConfiguration.Initialize();
			Application.Run(new Form1());
		}
	}
}