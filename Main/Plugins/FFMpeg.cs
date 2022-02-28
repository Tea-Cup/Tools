using System;
using System.Collections.Generic;
using System.IO;
using Tools.Shared;

namespace Tools.Plugins {
	public class FFMpeg : Plugin {
		public override string Command => "ffmpeg";
		public override IEnumerable<string> Aliases {
			get {
				yield return "ff";
			}
		}
		public override string Name => "FFMpeg";
		public override string Description => "FFMpeg short commands";

		[Option("format", 'f', "Resulting format (extension) with or without a leading period.\nIgnored if \"--output\" is specified (default: .mp4)")]
		public string Format { get; set; } = ".mp4";

		[Option("output", 'o', "Output filepath.")]
		public string Output { get; set; } = "";

		[RestOption("input", "Input filepath.")]
		public string[] Input { get; set; } = Array.Empty<string>();

		[Option("bitrate", 'b', "Target video bitrate.")]
		public string Bitrate { get; set; } = "";

		[Option("filter", 'f', "Filter string.")]
		public string Filter { get; set; } = "";

		[Option("scale", 's', "Target scale string.\nIgnored if \"--filter\" is specified.")]
		public string Scale { get; set; } = "";

		[Option("inspect", 'i', "Inspect input file.\nAll other options are ignored.")]
		public bool Inspect { get; set; } = false;

		private int Mpeg(ConfigBlock config) {
			if (Input.Length == 0) {
				Console.WriteLine("Input filename is empty");
				return 1;
			}
			if (string.IsNullOrEmpty(Output) && string.IsNullOrEmpty(Format)) {
				Console.WriteLine("Format and output are empty");
				return 1;
			}

			string input = Input[0];
			string output = string.IsNullOrEmpty(Output) ? Path.ChangeExtension(input, Format) : Output;
			if (input.ToLowerInvariant() == output.ToLowerInvariant()) {
				Console.WriteLine("input and output files are the same");
				return 1;
			}

			ProcessRunner runner = new() { Filename = config["Path"].Or("ffmpeg.exe") };

			if (!string.IsNullOrEmpty(Filter))
				runner.PatternValues.Add("filter", "-filter_complex", Filter);
			else if (!string.IsNullOrEmpty(Scale))
				runner.PatternValues.Add("filter", "-vf", $"scale={Scale}");
			if (!string.IsNullOrEmpty(Bitrate))
				runner.PatternValues.Add("bitrate", "-b:v", Bitrate);
			runner.PatternValues.Add("input", input);
			runner.PatternValues.Add("output", output);

			runner.Arguments = config["CommandLine"].Or("-hide_banner -i %input% %filter% %bitrate% -y %output%");

			return runner.Run().ExitCode;
		}
		private int Probe(ConfigBlock config) {
			if (Input.Length == 0) {
				Console.WriteLine("Input filename is empty");
				return 1;
			}

			string input = Input[0];

			ProcessRunner runner = new() { Filename = config["InspectPath"].Or("ffprobe.exe") };
			runner.PatternValues.Add("input", input);

			runner.Arguments = config["InspectCommandLine"].Or("-hide_banner -i %input%");

			return runner.Run().ExitCode;
		}

		public override int Run(ConfigBlock config) {
			return Inspect ? Probe(config) : Mpeg(config);
		}
	}
}
