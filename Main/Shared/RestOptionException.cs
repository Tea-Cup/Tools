namespace Tools.Shared {
	public class RestOptionException : OptionException {
		public new RestOptionAttribute Option { get; }
		public RestOptionException(RestOptionAttribute option, string message) : base(message) {
			Option = option;
		}
	}
}
