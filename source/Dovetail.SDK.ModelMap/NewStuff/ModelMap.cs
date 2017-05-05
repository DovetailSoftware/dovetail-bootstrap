﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
	public class ModelMap : IExpandableMap
    {
	    private readonly string _name;
        private readonly IList<IModelMapInstruction> _instructions = new List<IModelMapInstruction>();

		public ModelMap(string name)
		{
			_name = name;
		}

	    public string Name
	    {
		    get { return _name; }
	    }

	    public void AddInstruction(IModelMapInstruction instruction)
        {
            _instructions.Add(instruction);
        }

        public void Accept(IModelMapVisitor visitor)
        {
            _instructions.Each(_ => _.Accept(visitor));
        }

		void IExpandableMap.Expand(IModelMapCache cache)
		{
			var replacements = new List<Tuple<int, IncludePartial>>();
			for (var i = 0; i < _instructions.Count; ++i)
			{
				var partial = _instructions[i] as IncludePartial;
				if (partial != null)
				{
					replacements.Add(new Tuple<int, IncludePartial>(i, partial));
				}
			}

			var offset = 0;
			foreach (var replacement in replacements)
			{
				var partial = cache.Partials().SingleOrDefault(_ => _.Name.EqualsIgnoreCase(replacement.Item2.Name));
				if (partial == null)
					continue;

				var index = replacement.Item1 + offset;
				_instructions.RemoveAt(index);

				var variables = replacement.Item2.Attributes.Select(pair => new ConfiguredVariable(pair.Key, pair.Value));
				var source = new ConfiguredVariableSource(variables);
				var expander = new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource> { source }), new InMemoryServiceLocator());

				var instructionsToAdd = partial
					._instructions
					.Where(_ => _.GetType() != typeof(BeginModelMap) && _.GetType() != typeof(EndModelMap))
					.ToArray();

				for (var i = 0; i < instructionsToAdd.Length; ++i)
				{
					var instruction = instructionsToAdd[i];
					var properties = instruction.GetType().GetProperties().Where(_ => _.PropertyType.IsString());
					foreach (var property in properties)
					{
						var value = (string) property.GetValue(instruction, null);
						if (value == null)
							continue;

						if (expander.IsVariable(value))
						{
							property.SetValue(instruction, expander.Expand(value));
						}
					}

					_instructions.Insert(index + i, instruction);
				}

				offset += instructionsToAdd.Length - 1;
			}
		}

		protected bool Equals(ModelMap other)
		{
			return string.Equals(_name, other._name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ModelMap)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (StringComparer.OrdinalIgnoreCase.GetHashCode(_name) * 397);
			}
		}

		public static bool operator ==(ModelMap left, ModelMap right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ModelMap left, ModelMap right)
		{
			return !Equals(left, right);
		}

		private class ConfiguredVariable : IMappingVariable
		{
			public ConfiguredVariable(string key, string value)
			{
				Key = key;
				Value = value;
			}

			public string Key { get; set; }
			public string Value { get; private set; }

			public bool Matches(string key)
			{
				return Key.EqualsIgnoreCase(key);
			}

			public object Expand(string key, IServiceLocator services)
			{
				return Value;
			}
		}

		private class ConfiguredVariableSource : IMappingVariableSource
		{
			private readonly IEnumerable<ConfiguredVariable> _variables;

			public ConfiguredVariableSource(IEnumerable<ConfiguredVariable> variables)
			{
				_variables = variables;
			}

			public IEnumerable<IMappingVariable> Variables()
			{
				return _variables;
			}
		}
	}
}