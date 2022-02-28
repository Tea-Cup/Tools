using System;

namespace Tools.Shared {
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
	public class RestOptionAttribute : Attribute {
		public string Name { get; }
		public string Description { get; }
		public RestOptionAttribute() : this("","") { }
		public RestOptionAttribute(string name) : this(name, "") { }
		public RestOptionAttribute(string name, string description) {
			Name = name;
			Description = description;
		}
	}
}
