using System;

namespace Tools.Shared {
	public class OptionException : Exception {
		public OptionAttribute Option { get; }
		public OptionException(string message) : this(null!, message) { }
		public OptionException(OptionAttribute option, string message) : base(message) {
			Option = option;
		}
	}
}
