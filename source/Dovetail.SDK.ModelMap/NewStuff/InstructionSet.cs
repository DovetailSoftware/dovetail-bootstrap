using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;

namespace Dovetail.SDK.ModelMap.NewStuff
{
	public class InstructionSet
	{
		private readonly int _start;
		private readonly IEnumerable<IModelMapInstruction> _instructions;
		private readonly int _end;

		public InstructionSet(int start, IEnumerable<IModelMapInstruction> instructions, int end)
		{
			_start = start;
			_instructions = instructions;
			_end = end;
		}

		public int Start
		{
			get { return _start; }
		}

		public IEnumerable<IModelMapInstruction> Instructions
		{
			get { return _instructions; }
		}

		public int End
		{
			get { return _end; }
		}
	}
}