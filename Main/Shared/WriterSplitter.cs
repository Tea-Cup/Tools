using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tools.Shared {
	public class WriterSplitter : TextWriter {
		public override Encoding Encoding { get; } = Encoding.Default;
		public IEnumerable<TextWriter> Targets { get; }

		public WriterSplitter(IEnumerable<TextWriter> targets) {
			Targets = new List<TextWriter>(targets);
		}
		public WriterSplitter(params TextWriter[] targets) : this((IEnumerable<TextWriter>)targets) { }

		public override void Close() {
			foreach (TextWriter target in Targets) target.Close();
		}
		protected override void Dispose(bool disposing) {
			foreach (TextWriter target in Targets) target.Dispose();
			base.Dispose(disposing);
		}
		public override void Flush() {
			foreach (TextWriter target in Targets) target.Flush();
		}

		public override void Write(char value) {
			foreach (TextWriter target in Targets) target.Write(value);
		}
	}
}
