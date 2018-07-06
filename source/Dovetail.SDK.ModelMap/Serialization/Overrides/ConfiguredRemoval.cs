using System;

namespace Dovetail.SDK.ModelMap.Serialization.Overrides
{
	public class ConfiguredRemoval
	{
		private readonly Type _instructionType;
		private readonly Func<ModelMap, string, int, InstructionSet> _find;

		public ConfiguredRemoval(Type instructionType, Func<ModelMap, string, int, InstructionSet> find)
		{
			_instructionType = instructionType;
			_find = find;
		}

		public Type InstructionType
		{
			get { return _instructionType; }
		}

		public Func<ModelMap, string, int, InstructionSet> Find
		{
			get { return _find; }
		}
	}
}