using System;
using System.Collections.Generic;
using Tools.Shared;

namespace Tools.Plugins {
	public class ImageOnTopPlugin : Plugin {
		public override string Command => "imageontop";
		public override IEnumerable<string> Aliases {
			get {
				yield return "iot";
			}
		}
		public override string Name => "Image On Top";
		public override string Description => "Open an image in an Always On Top Window";

		[RestOption("image", "Image path. If empty, opens a file dialog.")]
		public string[] Image { get; set; } = Array.Empty<string>();

		public override int Run(ConfigBlock config) {
			ProcessRunner p = new() { Filename = config["Path"].Or("iot.exe") };
			if (Image.Length > 0) p.ArgumentsList.Add(Image[0]);
			p.Run(false);
			return 0;
		}
	}
}
