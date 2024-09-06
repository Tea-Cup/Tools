using System;
using System.Collections.Generic;

namespace Tools.Shared {
	public abstract class Plugin {
		public abstract string Command { get; }
		public abstract IEnumerable<string> Aliases { get; }
		public abstract string Name { get; }
		public abstract string Description { get; }

		public bool CommandletMatch(string cmd) {
			cmd = cmd.ToLowerInvariant();
			if (Command.ToLowerInvariant() == cmd) return true;
			foreach(string s in Aliases) {
				if (s.Trim().ToLowerInvariant() == cmd) return true;
			}
			return false;
		}

		public abstract int Run(ConfigBlock config);
	}
}
