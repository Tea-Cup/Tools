using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Tools.Shared {
	public class ProcessRunner {
		private static readonly Regex rxArguments = new(@"[^\s""']+|""([^""]*)""|'([^']*)'", RegexOptions.Compiled);
		private static readonly HashSet<Process> children = new();
		private static readonly Thread interruptThread;

		static ProcessRunner() {
			interruptThread = new(InterruptLoop) { Name = "interruptThread", IsBackground = true };
			interruptThread.Start();
		}
		private static void InterruptLoop() {
			Console.TreatControlCAsInput = true;
			while (true) {
				ConsoleKeyInfo key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
					break;
			}
			foreach (Process process in children) {
				process.Kill(true);
			}
			Environment.Exit(-1);
		}
		private static void StartProcess(Process p) {
			children.Add(p);
			p.Start();
		}
		private static void WaitProcess(Process p) {
			p.WaitForExit();
			children.Remove(p);
		}

		public List<string> ArgumentsList { get; } = new();
		public string Arguments {
			get => string.Join(' ', ArgumentsList.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
			set {
				ArgumentsList.Clear();
				foreach (string arg in SplitArguments(value)) {
					string[] realValue = new[] { arg };
					if (arg.StartsWith('%') && arg.EndsWith('%') && arg.Length > 2 && !arg.StartsWith("%%")) {
						string argName = arg[1..^1];
						if (!PatternValues.TryGetValue(argName, out string[]? val))
							continue;
						realValue = val;
					}
					ArgumentsList.AddRange(realValue);
				}
			}
		}
		public string Filename { get; set; } = "";
		public PatternValuesCollection PatternValues { get; } = new();

		public static IEnumerable<string> SplitArguments(string args) {
			MatchCollection matches = rxArguments.Matches(args);
			foreach (Match match in matches) {
				yield return (match.Groups[1], match.Groups[2]) switch {
					(var a, var b) when a.Success => a.Value,
					(var a, var b) when b.Success => b.Value,
					_ => match.Value
				};
			}
		}

		public ProcessResult Run(bool wait = true) {
			Process p = new() {
				StartInfo = new() {
					Arguments = Arguments,
					FileName = Filename,
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				}
			};
			Console.WriteLine($"> {Filename} {Arguments}");
			Console.WriteLine();
			if (wait) {
				using StringWriter strOut = new();
				using StringWriter strErr = new();
				using WriterSplitter stdout = new(Console.Out, strOut);
				using WriterSplitter stderr = new(Console.Error, strErr);
				object sync = new();
				p.OutputDataReceived += (s, e) => { lock (sync) { stdout.WriteLine(e.Data); } };
				p.ErrorDataReceived += (s, e) => { lock (sync) { stderr.WriteLine(e.Data); } };
				StartProcess(p);
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				WaitProcess(p);
				return new(p.ExitCode, strOut.ToString(), strErr.ToString());
			} else {
				p.Start();
				return new(0, "", "");
			}
		}

		public static ProcessResult Run(string filename, IEnumerable<string> args, bool wait = true) {
			ProcessRunner runner = new() { Filename = filename };
			runner.ArgumentsList.AddRange(args);
			return runner.Run(wait);
		}
		public static ProcessResult Run(string filename, string args, bool wait = true) {
			ProcessRunner runner = new() { Filename = filename };
			runner.Arguments = args;
			return runner.Run(wait);
		}
		public static ProcessResult Run(string filename, params string[] args) {
			return Run(filename, (IEnumerable<string>)args);
		}
		public static ProcessResult Run(string filename, bool wait, params string[] args) {
			return Run(filename, args, wait);
		}

		public class PatternValuesCollection : ICollection<KeyValuePair<string, string[]>> {
			private readonly Dictionary<string, string[]> dict = new();

			public int Count => dict.Count;
			public bool IsReadOnly => false;

			public bool TryGetValue(string key, [MaybeNullWhen(false)] out string[] value) {
				return dict.TryGetValue(key, out value);
			}

			public void Add(string key, params string[] values) {
				if (dict.ContainsKey(key)) dict[key] = values;
				else dict.Add(key, values);
			}

			public void Add(KeyValuePair<string, string[]> item) {
				dict.Add(item.Key, item.Value);
			}

			public void Clear() {
				dict.Clear();
			}

			public bool Contains(KeyValuePair<string, string[]> item) {
				return dict.Contains(item);
			}
			public bool ContainsKey(string key) {
				return dict.ContainsKey(key);
			}

			public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex) {
				((ICollection<KeyValuePair<string, string[]>>)dict).CopyTo(array, arrayIndex);
			}


			public bool Remove(KeyValuePair<string, string[]> item) {
				return ((ICollection<KeyValuePair<string, string[]>>)dict).Remove(item);
			}
			public bool Remove(string key) {
				return dict.Remove(key);
			}

			public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator() {
				return dict.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return dict.GetEnumerator();
			}
		}
	}

	public record ProcessResult(int ExitCode, string Output, string Error);
}
