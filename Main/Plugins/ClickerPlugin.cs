using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tools.Shared;

namespace Tools.Plugins {
	public class ClickerPlugin : Plugin {
		public override string Command => "clicker";
		public override IEnumerable<string> Aliases { get { yield break; } }
		public override string Name => "Clicker";
		public override string Description => "Repeatedly click left mouse button";

		[Option("delay", 'd', "Click delay (Default: 10s)\nAvailable suffixes: ms, s, m, h")]
		public string Delay { get; set; } = "10s";

		[DllImport("user32")]
		private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);
		[DllImport("user32")]
		private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

		private static Regex DelayRegex = new(@"^(\d+)(ms|s|m|h)$", RegexOptions.IgnoreCase);
		public override int Run(ConfigBlock config) {
			Match m = DelayRegex.Match(Delay);
			if (!m.Success) throw new OptionException("Invalid delay");

			int value = int.Parse(m.Groups[1].Value);
			TimeSpan delay = m.Groups[2].Value switch {
				"ms" => TimeSpan.FromMilliseconds(value),
				"s" => TimeSpan.FromSeconds(value),
				"m" => TimeSpan.FromMinutes(value),
				"h" => TimeSpan.FromHours(value),
				_ => throw new OptionException("Unknown delay suffix")
			};

			Console.WriteLine("Delay: " + delay);

			Task.Run(async () => {
				Stopwatch sw = new();
				sw.Start();
				Console.WriteLine("[{0}] Started!", sw.Elapsed);
				while (true) {
					await Task.Delay(delay);
					Console.WriteLine("[{0}] Click!", sw.Elapsed);
					mouse_event(0x2, 0, 0, 0, IntPtr.Zero);
					await Task.Delay(100);
					mouse_event(0x4, 0, 0, 0, IntPtr.Zero);
				}
			});
			MessageBox(IntPtr.Zero, "Press OK to stop clicking", "Click-click~", 0x00040010);
			return 0;
		}
	}
}
