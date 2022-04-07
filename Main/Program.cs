using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Tools.Shared;
using static System.Console;

namespace Tools {
	public static class Program {
		public static string ExePath { get; } = Process.GetCurrentProcess().MainModule!.FileName!;
		public static string ExeName { get; } = Path.GetFileNameWithoutExtension(ExePath);
		public static string ExeDir { get; } = Path.GetDirectoryName(ExePath) ?? ".";

		private static readonly string[] hostHelp = new string[] {
			"Usage:",
			"list",
			"    Display loaded tools list.",
			"",
			"help|?",
			"    Display this info.",
			"",
			"help|? <cmd>",
			"    Display info about a tool with specified command.",
			"",
			"<cmd> [...]",
			"    Execute command with specified arguments.",
			"All commands accept config file: --config=<path>"
		};
		private static string IndentLines(string txt, string indent) {
			return string.Join("\n" + indent, txt.Split('\n'));
		}
		public static void DisplayPlugins(IEnumerable<Plugin> plugins) {
			WriteLine("List of loaded tools:");
			foreach (Plugin plugin in plugins) {
				Write($"{plugin.Name}: {plugin.Command}");
				string aliases = string.Join(", ", plugin.Aliases);
				if (aliases.Length > 0) Write($" ({aliases})");
				WriteLine();
				WriteLine($"    {IndentLines(plugin.Description, "    ")}");
			}
		}
		public static void DisplayHelp(Plugin? plugin) {
			if (plugin is null) {
				foreach (string s in hostHelp)
					WriteLine(s.Replace("{exe}", ExeName));
				return;
			}

			WriteLine($"Tool:    {plugin.Name}");
			WriteLine($"Command: {plugin.Command}");
			WriteLine($"Aliases: {string.Join(", ", plugin.Aliases)}");
			WriteLine($"    {IndentLines(plugin.Description, "    ")}");
			WriteLine();

			List<string> usage = new();
			List<(string, string, string)> options = new();
			foreach ((OptionAttribute option, Type type) in CommandLine.GetOptions(plugin.GetType())) {
				(char lb, char rb) = GetBrackets(option.IsRequired);
				string value = type == typeof(string) ? "=<string>"
					: type == typeof(int) ? "=<int>"
					: "";
				usage.Add($"{lb}--{option.LongName}{value}{rb}");
				options.Add((
					option.ShortName > 0 ? $"-{option.ShortName}" : "",
					$"--{option.LongName}",
					option.Description
				));
			}

			if (options.Count == 0) return;

			if (CommandLine.GetRestOption(plugin.GetType()) is RestOptionAttribute rest) {
				usage.Add($"[...{rest.Name}]");
				options.Add(("", $"...{rest.Name}", rest.Description));
			}

			WriteLine($"Usage: {string.Join(' ', usage)}");
			WriteLine();

			int shortLength = options.Max(x => x.Item1.Length);
			int longLength = options.Max(x => x.Item2.Length);
			string indent = new(' ', shortLength + longLength + 5);
			foreach ((string shortName, string longName, string desc) in options) {
				Write(shortName.PadRight(shortLength, ' '));
				Write(shortName.Length > 0 ? ", " : "  ");
				Write(longName.PadRight(longLength, ' '));
				Write(desc.Length > 0 ? " - " : "   ");
				WriteLine(IndentLines(desc, indent));
			}

			static (char, char) GetBrackets(bool required) {
				return required ? ('<', '>') : ('[', ']');
			}
		}

		public static int Main(string[] args) {
			List<Plugin> plugins = new();

			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
				if (type == typeof(Plugin)) continue;
				if (type.IsAssignableTo(typeof(Plugin)))
					plugins.Add((Plugin)Activator.CreateInstance(type)!);
			}

			if (args.Length == 0) {
				DisplayHelp(null);
				return 0;
			}

			string cmd = args[0];
			string[] rest = args[1..];

			if (cmd is "list") {
				DisplayPlugins(plugins);
				return 0;
			}
			if (cmd is "help" or "?") {
				Plugin? pl = null;
				if (rest.Length > 0) {
					pl = plugins.FirstOrDefault(x => x.CommandletMatch(rest[0]));
					if (pl is null) {
						WriteLine($"Unknown command: {rest[0]}");
						DisplayPlugins(plugins);
						return 1;
					}
				}
				DisplayHelp(pl);
				return 0;
			}

			Plugin? plugin = plugins.FirstOrDefault(x => x.CommandletMatch(cmd));
			if (plugin is null) {
				WriteLine($"Unknown command: {cmd}");
				DisplayPlugins(plugins);
				return 0;
			}

			ConfigBlock? cfg = CommandLine.Parse(rest, plugin);
			if (cfg is null) {
				new Config(Path.Combine(ExeDir, $"{ExeName}.config.ini")).TryGetValue(plugin.Command, out cfg);
			}
			if (cfg is null) {
				cfg = new(plugin.Command, Array.Empty<KeyValuePair<string, string>>());
			}

			return plugin.Run(cfg);
		}
	}
}
