using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Tools.Shared {
	public class Config : IReadOnlyDictionary<string, ConfigBlock> {
		private Dictionary<string, ConfigBlock> Dict { get; } = new();

		public Config(string path) {
			using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			using StreamReader sr = new(fs);

			string block = "";
			Dictionary<string, string> values = new();
			while (!sr.EndOfStream) {
				if (sr.ReadLine() is not string line) break;
				if (line.TrimStart().StartsWith(';')) continue;

				string? name = null;
				string? value = null;

				if (TryParseName(line, out name)) {
					Dict.Add(block, new(block, values));
					block = name;
					values = new();
				} else if (TryParseValue(line, out name, out value)) {
					values.Add(name, value);
				}
			}

			Dict.Add(block, new(block, values));
		}

		private static bool TryParseName(string line, [NotNullWhen(true)] out string? name) {
			line = line.Trim();
			if (!line.StartsWith('[') || !line.EndsWith(']')) {
				name = null;
				return false;
			}
			name = line[1..^1].Trim();
			return true;
		}
		private static bool TryParseValue(string line, [NotNullWhen(true)] out string? name, [NotNullWhen(true)] out string? value) {
			string[] split = line.Split('=');
			if (split.Length != 2) {
				name = null;
				value = null;
				return false;
			}
			name = split[0].Trim();
			value = string.Join('=', split[1..]).Trim();
			return true;
		}

		#region IReadOnlyDictionary implementation over Dict
		public IEnumerable<string> Keys => Dict.Keys;
		public IEnumerable<ConfigBlock> Values => Dict.Values;
		public int Count => Dict.Count;

		public ConfigBlock this[string key] => Dict[key];

		public bool ContainsKey(string key) {
			return Dict.ContainsKey(key);
		}

		public bool TryGetValue(string key, [MaybeNullWhen(false)] out ConfigBlock value) {
			return Dict.TryGetValue(key, out value);
		}

		public IEnumerator<KeyValuePair<string, ConfigBlock>> GetEnumerator() {
			return Dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return Dict.GetEnumerator();
		}
		#endregion
	}

	public class ConfigBlock : IReadOnlyDictionary<string, PossibleConfigValue> {
		private Dictionary<string, PossibleConfigValue> Dict { get; }
		public string Name { get; }

		public ConfigBlock(string name, IEnumerable<KeyValuePair<string, string>> values) {
			Name = name;
			Dict = new(values.Select(x => new KeyValuePair<string, PossibleConfigValue>(x.Key, new(x.Value))));
		}

		#region IReadOnlyDictionary implementation over Dict
		public IEnumerable<string> Keys => Dict.Keys;
		public IEnumerable<PossibleConfigValue> Values => Dict.Values;
		public int Count => Dict.Count;

		public PossibleConfigValue this[string key] => Dict[key];


		public bool ContainsKey(string key) {
			return Dict.ContainsKey(key);
		}

		public bool TryGetValue(string key, [MaybeNullWhen(false)] out PossibleConfigValue value) {
			return Dict.TryGetValue(key, out value);
		}
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) {
			if (TryGetValue(key, out PossibleConfigValue? val) && val.Exists) {
				value = val.Value!;
				return true;
			}
			value = null;
			return false;
		}

		public IEnumerator<KeyValuePair<string, PossibleConfigValue>> GetEnumerator() {
			return Dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return Dict.GetEnumerator();
		}
		#endregion
	}

	public class PossibleConfigValue {
		public bool Exists { get; } = false;
		public string? Value { get; } = null;

		public PossibleConfigValue() { }
		public PossibleConfigValue(string value) {
			Exists = true;
			Value = value;
		}

		public string Or(string value) {
			if (Exists) return Value!;
			return value;
		}

		public static implicit operator string?(PossibleConfigValue value) {
			return value.Value;
		}
	}
}
