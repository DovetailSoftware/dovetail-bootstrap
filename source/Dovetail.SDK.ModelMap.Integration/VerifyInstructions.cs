using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Integration
{
	public class VerifyInstructions
	{
		public static void Assert(IEnumerable<IModelMapInstruction> instructions, Action<VerificationExpression> configure)
		{
			configure(new VerificationExpression(instructions.ToArray()));
		}

		public class VerificationExpression
		{
			private readonly IModelMapInstruction[] _instructions;
			private int _index;

			public VerificationExpression(IModelMapInstruction[] instructions)
			{
				_instructions = instructions;
				_index = 0;
			}

			public TInstruction Get<TInstruction>() where TInstruction : IModelMapInstruction
			{
				var instruction = _instructions[_index];
				_index++;

				return instruction.As<TInstruction>();
			}

			public void Is<TInstruction>() where TInstruction : IModelMapInstruction
			{
				_instructions[_index].IsType<TInstruction>();
				_index++;
			}

			public void Verify<TInstruction>(Action<TInstruction> action) where TInstruction : IModelMapInstruction
			{
				action(Get<TInstruction>());
			}

			public void Skip(int length)
			{
				_index += length;
			}
		}
	}
}