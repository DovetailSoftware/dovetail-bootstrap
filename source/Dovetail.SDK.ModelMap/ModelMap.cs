using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
	public class ModelMap : IExpandableMap
    {
	    private readonly string _name;
		private readonly string _entity;
        private readonly IList<IModelMapInstruction> _instructions = new List<IModelMapInstruction>();

		public ModelMap(string name, string entity)
		{
			_name = name;
			_entity = entity;
		}

		public string Name
	    {
		    get { return _name; }
	    }

		public string Entity
		{
			get { return _entity; }
		}

		public IModelMapInstruction[] Instructions { get { return _instructions.ToArray(); } }

		public void AddInstruction(IModelMapInstruction instruction)
        {
            _instructions.Add(instruction);
        }

		public void InsertInstruction(int index,IModelMapInstruction instruction)
		{
			_instructions.Insert(index, instruction);
		}

		public void InsertInstructions(int index, IEnumerable<IModelMapInstruction> instructions)
		{
			var position = index;
			foreach (var instruction in instructions)
			{
				InsertInstruction(position, instruction);
				position++;
			}
		}

		public void RemoveInstruction(IModelMapInstruction instruction)
		{
			_instructions.Remove(instruction);
		}

		public void Accept(IModelMapVisitor visitor)
        {
            _instructions.Each(_ => _.Accept(visitor));
        }

		public InstructionSet FindProperty(string key, int startIndex)
		{
			var instructions = new List<IModelMapInstruction>();
			int count = 0;
			int start = 0;
			int end = 0;
			var collecting = false;
			for (var i = 0; i < _instructions.Count; ++i)
			{
				var instruction = _instructions[i];
				var beginProp = instruction as BeginProperty;
				if (beginProp != null && beginProp.Key.Resolve(null).ToString().EqualsIgnoreCase(key) && i > startIndex)
				{
					start = i;
					collecting = true;
				}

				if (collecting)
				{
					instructions.Add(instruction);

					var endProp = instruction as EndProperty;
					if (endProp != null)
					{
						--count;
					}
					else if (beginProp != null)
					{
						++count;
					}

					if (count == 0)
					{
						end = i;
						break;
					}
				}
			}

			return new InstructionSet(start, instructions, end);
		}

		public InstructionSet FindMappedProperty(string key, int startIndex)
		{
			var instructions = new List<IModelMapInstruction>();
			int count = 0;
			int start = 0;
			int end = 0;
			var collecting = false;
			for (var i = 0; i < _instructions.Count; ++i)
			{
				var instruction = _instructions[i];
				var beginProp = instruction as BeginMappedProperty;
				if (beginProp != null && beginProp.Key.Resolve(null).ToString().EqualsIgnoreCase(key) && i > startIndex)
				{
					start = i;
					collecting = true;
				}

				if (collecting)
				{
					instructions.Add(instruction);

					var endProp = instruction as EndMappedProperty;
					if (endProp != null)
					{
						--count;
					}
					else if (beginProp != null)
					{
						++count;
					}

					if (count == 0)
					{
						end = i;
						break;
					}
				}
			}

			return new InstructionSet(start, instructions, end);
		}

		public InstructionSet FindMappedCollection(string key, int startIndex)
		{
			var instructions = new List<IModelMapInstruction>();
			int count = 0;
			int start = 0;
			int end = 0;
			var collecting = false;
			for (var i = 0; i < _instructions.Count; ++i)
			{
				var instruction = _instructions[i];
				var beginProp = instruction as BeginMappedCollection;
				if (beginProp != null && beginProp.Key.Resolve(null).ToString().EqualsIgnoreCase(key) && i > startIndex)
				{
					start = i;
					collecting = true;
				}

				if (collecting)
				{
					instructions.Add(instruction);

					var endProp = instruction as EndMappedCollection;
					if (endProp != null)
					{
						--count;
					}
					else if (beginProp != null)
					{
						++count;
					}

					if (count == 0)
					{
						end = i;
						break;
					}
				}
			}

			return new InstructionSet(start, instructions, end);
		}

		public void Remove(InstructionSet set, int offset)
		{
			for (var i = set.Start - offset; i <= set.End - offset; ++i)
			{
				_instructions.RemoveAt(set.Start - offset);
			}
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

				partial.As<IExpandableMap>().Expand(cache);

				var index = replacement.Item1 + offset;
				_instructions.RemoveAt(index);

				_instructions.Insert(index, new PushVariableContext
				{
					Attributes = replacement.Item2.Attributes
				});

				offset++;
				index++;

				var instructionsToAdd = partial
					._instructions
					.Where(_ => _.GetType() != typeof(BeginModelMap) && _.GetType() != typeof(EndModelMap))
					.Select(CloneInstruction)
					.ToArray();

				for (var i = 0; i < instructionsToAdd.Length; ++i)
				{
					var instruction = instructionsToAdd[i];
					var targetIndex = index + i;
					if (targetIndex > _instructions.Count)
						_instructions.Add(instruction);
					else
						_instructions.Insert(index + i, instruction);
				}

				offset += instructionsToAdd.Length - 1;
				var endIndex = index + instructionsToAdd.Length;

				_instructions.Insert(endIndex, new PopVariableContext());
				offset++;
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

		public static IModelMapInstruction CloneInstruction(IModelMapInstruction instruction)
		{
			var cloneable = instruction as ICloneable;
			if (cloneable != null)
				return (IModelMapInstruction) cloneable.Clone();

			var type = instruction.GetType();
			var clone = (IModelMapInstruction) FastYetSimpleTypeActivator.CreateInstance(type);
			var properties = type.GetProperties();

			foreach (var property in properties)
			{
				if (!property.CanWrite)
					continue;

				property.SetValue(clone, property.GetValue(instruction, null));
			}

			return clone;
		}
	}
}