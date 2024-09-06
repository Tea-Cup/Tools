using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tools.Shared;

namespace Tools {
	public class YoutubeDL : Plugin {
		public override string Command => "youtube-dl";
		public override IEnumerable<string> Aliases {
			get {
				yield return "yt";
				yield return "ytdl";
				yield return "yt-dl";
				yield return "youtube";
				yield return "youtubedl";
			}
		}
		public override string Name => "Youtube-DL";
		public override string Description => "Download media from YouTube";

		[Option("only-audio", 'a', "download only audio")]
		public bool OnlyAudio { get; set; } = false;

		[RestOption("urls", "list of YouTube URL's")]
		public string[] Urls { get; set; } = Array.Empty<string>();

		public override int Run(ConfigBlock config) {
			if(Urls.Length == 0) {
				Console.WriteLine("No urls specified.");
				return 1;
			}
			ProcessRunner runner = new() { Filename = config["Path"].Or("youtube-dl.exe") };
			string args = config["CommandLine"].Or("%only-audio% %input%");
			if (OnlyAudio)
				runner.PatternValues.Add("only-audio", "--extract-audio");

			foreach (string url in Urls) {
				runner.PatternValues.Add("input", url);
				runner.Arguments = args;
				runner.Run();
			}

			return 0;
		}
	}
}
