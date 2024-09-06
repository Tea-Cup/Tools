using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Tools.Shared {
	public static class CommandLine {
		private static readonly Regex shortRegex = new(@"^-([\w\d])(?:=(?:""([^""]*)""|(.*)))?$", RegexOptions.Compiled);
		private static readonly Regex longRegex = new(@"^--([\w\d-]+)(?:=(?:""([^""]*)""|(.*)))?$", RegexOptions.Compiled);

		public static ConfigBlock? Parse(string[] args, Plugin target) {
			(TargetHandler[] handlers, TargetHandler? restHandler) = GetHandlers(target.GetType());
			List<string> restArgs = new();
			ConfigBlock? config = null;

			foreach (string arg in args) {
				Match shortMatch = shortRegex.Match(arg);
				if (shortMatch.Success) {
					(char[] head, char tail) = shortMatch.Groups[1].Value.ToCharArray();
					string value = shortMatch.Groups[2].Success ? shortMatch.Groups[2].Value
						: shortMatch.Groups[3].Success ? shortMatch.Groups[3].Value
						: "";
					foreach (char c in head) {
						TargetHandler? handler = handlers.FirstOrDefault(x => x.ShortName == c);
						if (handler is null) continue;
						handler.Set(target, true);
					}
					{
						TargetHandler? handler = handlers.FirstOrDefault(x => x.ShortName == tail);
						if (handler is null) continue;
						handler.Set(target, value);
					}
					continue;
				}

				Match longMatch = longRegex.Match(arg);
				if (longMatch.Success) {
					string name = longMatch.Groups[1].Value.ToLowerInvariant();
					string value = longMatch.Groups[2].Success ? longMatch.Groups[2].Value
						: longMatch.Groups[3].Success ? longMatch.Groups[3].Value
						: "";
					if (name == "config") {
						new Config(value).TryGetValue(target.Command, out config);
						continue;
					}
					TargetHandler? handler = handlers.FirstOrDefault(x => x.Name == name);
					if (handler is null) continue;
					handler.Set(target, value);
					continue;
				}

				restArgs.Add(arg);
			}

			restHandler?.Set(target, restArgs.ToArray());
			return config;
		}

		public static IEnumerable<(OptionAttribute, Type)> GetOptions(Type type) {
			foreach (PropertyInfo property in type.GetProperties()) {
				OptionAttribute? option = property.GetCustomAttribute<OptionAttribute>();
				if (option is null) continue;
				TargetHandler.ValidateProperty(option, property);
				yield return (option, property.PropertyType);
			}
			foreach (FieldInfo field in type.GetFields()) {
				OptionAttribute? option = field.GetCustomAttribute<OptionAttribute>();
				if (option is null) continue;
				TargetHandler.ValidateField(option, field);
				yield return (option, field.FieldType);
			}
			foreach (MethodInfo method in type.GetMethods()) {
				OptionAttribute? option = method.GetCustomAttribute<OptionAttribute>();
				if (option is null) continue;
				TargetHandler.ValidateMethod(option, method);
				yield return (option, method.GetParameters().Length == 0 ? typeof(void) : method.GetParameters()[0].ParameterType);
			}
		}
		public static RestOptionAttribute? GetRestOption(Type type) {
			RestOptionAttribute? result = null;
			foreach (PropertyInfo property in type.GetProperties()) {
				RestOptionAttribute? rest = property.GetCustomAttribute<RestOptionAttribute>();
				if (rest is null) continue;
				TargetHandler.ValidateProperty(rest, property);
				result = rest;
			}
			foreach (FieldInfo field in type.GetFields()) {
				RestOptionAttribute? rest = field.GetCustomAttribute<RestOptionAttribute>();
				if (rest is null) continue;
				TargetHandler.ValidateField(rest, field);
				result = rest;
			}
			foreach (MethodInfo method in type.GetMethods()) {
				RestOptionAttribute? rest = method.GetCustomAttribute<RestOptionAttribute>();
				if (rest is null) continue;
				TargetHandler.ValidateMethod(rest, method);
				result = rest;
			}
			return result;
		}

		private static (TargetHandler[] handlers, TargetHandler? restHandler) GetHandlers(Type type) {
			List<TargetHandler> handlers = new();
			TargetHandler? restHandler = null;

			foreach (PropertyInfo property in type.GetProperties()) {
				OptionAttribute? option = property.GetCustomAttribute<OptionAttribute>();
				if (option is not null) {
					TargetHandler.ValidateProperty(option, property);
					handlers.Add(new(option, property));
				}
				RestOptionAttribute? rest = property.GetCustomAttribute<RestOptionAttribute>();
				if (rest is not null) {
					TargetHandler.ValidateProperty(rest, property);
					restHandler = new(rest, property);
				}
			}
			foreach (FieldInfo field in type.GetFields()) {
				OptionAttribute? option = field.GetCustomAttribute<OptionAttribute>();
				if (option is not null) {
					TargetHandler.ValidateField(option, field);
					handlers.Add(new(option, field));
				}
				RestOptionAttribute? rest = field.GetCustomAttribute<RestOptionAttribute>();
				if (rest is not null) {
					TargetHandler.ValidateField(rest, field);
					restHandler = new(rest, field);
				}
			}
			foreach (MethodInfo method in type.GetMethods()) {
				OptionAttribute? option = method.GetCustomAttribute<OptionAttribute>();
				if (option is not null) {
					TargetHandler.ValidateMethod(option, method);
					handlers.Add(new(option, method));
				}
				RestOptionAttribute? rest = method.GetCustomAttribute<RestOptionAttribute>();
				if (rest is not null) {
					TargetHandler.ValidateMethod(rest, method);
					restHandler = new(rest, method);
				}
			}

			return (handlers.ToArray(), restHandler);
		}

		private static void Deconstruct<T>(this T[] arr, out T[] head, out T tail) {
			if (arr.Length == 0) {
				head = Array.Empty<T>();
				tail = default!;
			} else if (arr.Length == 1) {
				head = Array.Empty<T>();
				tail = arr[0];
			} else {
				head = arr[..^1];
				tail = arr[^1];
			}
		}

		private class TargetHandler {
			private static readonly Type[] allowed = new Type[] {
				typeof(string), typeof(int), typeof(bool)
			};

			public string Name { get; }
			public char? ShortName { get; }
			public string? Description { get; }
			public bool IsRequired { get; }
			public PropertyInfo? Property { get; }
			public FieldInfo? Field { get; }
			public MethodInfo? Method { get; }
			public bool IsSet { get; private set; }

			private TargetHandler(bool required, string name, char? shortName, string? description) {
				IsRequired = required;
				Name = name.ToLowerInvariant();
				ShortName = shortName;
				Description = description;
			}
			private TargetHandler(OptionAttribute option) : this(option.IsRequired, option.LongName, option.ShortName, option.Description) { }
			private TargetHandler(RestOptionAttribute option) : this(false, "", null, option.Description) { }
			public TargetHandler(OptionAttribute option, PropertyInfo prop) : this(option) { Property = prop; }
			public TargetHandler(OptionAttribute option, FieldInfo field) : this(option) { Field = field; }
			public TargetHandler(OptionAttribute option, MethodInfo method) : this(option) { Method = method; }
			public TargetHandler(RestOptionAttribute option, PropertyInfo prop) : this(option) { Property = prop; }
			public TargetHandler(RestOptionAttribute option, FieldInfo field) : this(option) { Field = field; }
			public TargetHandler(RestOptionAttribute option, MethodInfo method) : this(option) { Method = method; }

			public void Set(object target, object value) {
				if (Property is not null) SetProperty(Property, target, value);
				else if (Field is not null) SetField(Field, target, value);
				else if (Method is not null) SetMethod(Method, target, value);
				else throw new InvalidOperationException("Handler has no targets");
				IsSet = true;
			}

			private static void SetProperty(PropertyInfo prop, object target, object value) {
				prop.SetValue(target, Convert(prop.PropertyType, value));
			}
			private static void SetField(FieldInfo field, object target, object value) {
				field.SetValue(target, Convert(field.FieldType, value));
			}
			private static void SetMethod(MethodInfo method, object target, object value) {
				object[] parm = method.GetParameters().Length == 0
					? Array.Empty<object>()
					: new object[] { Convert(method.GetParameters()[0].ParameterType, value) };
				method.Invoke(target, parm);
			}

			private static object Convert(Type type, object value) {
				if (type == typeof(string)) return value.ToString()!;
				if (type == typeof(int)) return int.Parse(value.ToString() ?? "");
				if (type == typeof(bool)) return true;
				if (type == typeof(string[])) {
					if (value is not string[])
						throw new InvalidOperationException($"Unsupported rest option type: {value?.GetType().FullName ?? "null"}");
					return value;
				}
				throw new InvalidOperationException($"Unsupported type: {type.FullName}");
			}

			public static void ValidateProperty(OptionAttribute option, PropertyInfo prop) {
				if (prop is null)
					throw new ArgumentNullException(nameof(prop));
				if (prop.SetMethod is null)
					throw new OptionException(option, $"property \"{prop.Name}\" does not have a setter");
				if (!allowed.Contains(prop.PropertyType))
					throw new OptionException(option, $"property \"{prop.Name}\" has an unsupported type: {prop.PropertyType.FullName}");
			}

			public static void ValidateField(OptionAttribute option, FieldInfo field) {
				if (field is null)
					throw new ArgumentNullException(nameof(field));
				if (!allowed.Contains(field.FieldType))
					throw new OptionException(option, $"field \"{field.Name}\" has an unsupported type: {field.FieldType.FullName}");
			}

			public static void ValidateMethod(OptionAttribute option, MethodInfo method) {
				if (method is null)
					throw new ArgumentNullException(nameof(method));
				if (method.GetParameters().Length > 1)
					throw new OptionException(option, $"method \"{method.Name}\" expects more than one parameter");
				if (method.GetParameters().Length == 1) {
					ParameterInfo parm = method.GetParameters()[0];
					if (!allowed.Contains(parm.ParameterType))
						throw new OptionException(option, $"parameter \"{parm.Name}\" of method \"{method.Name}\" has an unsupported type: {parm.ParameterType.FullName}");
				}
			}

			public static void ValidateProperty(RestOptionAttribute option, PropertyInfo prop) {
				if (prop is null)
					throw new ArgumentNullException(nameof(prop));
				if (prop.SetMethod is null)
					throw new RestOptionException(option, $"property \"{prop.Name}\" does not have a setter");
				if (prop.PropertyType != typeof(string[]))
					throw new RestOptionException(option, $"property \"{prop.Name}\" has an unsupported type: {prop.PropertyType.FullName}");
			}

			public static void ValidateField(RestOptionAttribute option, FieldInfo field) {
				if (field is null)
					throw new ArgumentNullException(nameof(field));
				if (field.FieldType != typeof(string[]))
					throw new RestOptionException(option, $"field \"{field.Name}\" has an unsupported type: {field.FieldType.FullName}");
			}

			public static void ValidateMethod(RestOptionAttribute option, MethodInfo method) {
				if (method is null)
					throw new ArgumentNullException(nameof(method));
				if (method.GetParameters().Length > 1)
					throw new RestOptionException(option, $"method \"{method.Name}\" expects more than one parameter");
				if (method.GetParameters().Length == 1) {
					ParameterInfo parm = method.GetParameters()[0];
					if (parm.ParameterType != typeof(string[]))
						throw new RestOptionException(option, $"parameter \"{parm.Name}\" of method \"{method.Name}\" has an unsupported type: {parm.ParameterType.FullName}");
				}
			}
		}
	}
}
