using System;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.Instructions;

namespace Dovetail.SDK.ModelMap.Serialization.Overrides
{
	public class ModelMapDiffOptions
	{
		private readonly IList<Type> _offsets = new List<Type>();
		private readonly IList<PropertyContext> _propertyContexts = new List<PropertyContext>();
		private readonly IList<ConfiguredRemoval> _removals = new List<ConfiguredRemoval>();

		public ModelMapDiffOptions()
		{
			Offset<BeginRelation>();
			Offset<BeginTable>();
			Offset<BeginView>();
			Offset<BeginAdHocRelation>();
			Offset<BeginMappedProperty>();
			Offset<BeginMappedCollection>();

			PropertyContext<BeginProperty, EndProperty>();
			PropertyContext<BeginMappedProperty, EndMappedProperty>();
			PropertyContext<BeginMappedCollection, EndMappedCollection>();

			Remove<Instructions.RemoveMappedCollection>((_, key, index) => _.FindMappedCollection(key, index));
			Remove<Instructions.RemoveMappedProperty>((_, key, index) => _.FindMappedProperty(key, index));
			Remove<Instructions.RemoveProperty>((_, key, index) => _.FindProperty(key, index));
		}

		public void Offset<TInstruction>()
			where TInstruction : IModelMapInstruction
		{
			_offsets.Add(typeof(TInstruction));
		}

		public void PropertyContext<TStart, TEnd>()
			where TStart : IModelMapInstruction
			where TEnd : IModelMapInstruction
		{
			_propertyContexts.Add(new PropertyContext(typeof(TStart), typeof(TEnd)));
		}

		public void Remove<TInstruction>(Func<ModelMap, string, int, InstructionSet> findInstructionSet)
			where TInstruction : class, IModelMapRemovalInstruction
		{
			_removals.Insert(0, new ConfiguredRemoval(typeof(TInstruction), findInstructionSet));
		}

		public void ClearAll()
		{
			_removals.Clear();
			_offsets.Clear();
			_propertyContexts.Clear();
		}

		public IEnumerable<ConfiguredRemoval> Removals
		{
			get { return _removals; }
		}

		public IEnumerable<Type> Offsets
		{
			get { return _offsets; }
		}

		public IEnumerable<PropertyContext> PropertyContexts
		{
			get { return _propertyContexts; }
		}
	}

	public class PropertyContext
	{
		private readonly Type _current;
		private readonly Type _waitFor;

		public PropertyContext(Type current, Type waitFor)
		{
			_current = current;
			_waitFor = waitFor;
		}

		public bool Matches(Type current)
		{
			return _current == current;
		}

		public IModelMapInstruction WaitFor()
		{
			return (IModelMapInstruction)FastYetSimpleTypeActivator.CreateInstance(_waitFor);
		}
	}
}