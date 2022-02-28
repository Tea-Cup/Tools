using System;

namespace Tools.Shared {
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
	public class OptionAttribute : Attribute {
		public string LongName { get; }
		public bool IsRequired { get; }
		public char ShortName { get; }
		public string Description { get; }

		public OptionAttribute(
			string longName,
			char shortName = '\0',
			string description = ""
		) : this(false, longName, shortName, description) { }
		public OptionAttribute(
			bool required,
			string longName,
			char shortName = '\0',
			string description = ""
		) {
			IsRequired = required;
			LongName = longName;
			ShortName = shortName;
			Description = description;
		}
	}
}
