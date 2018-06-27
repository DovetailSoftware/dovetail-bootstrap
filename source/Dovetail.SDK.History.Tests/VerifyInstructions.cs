using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.History.Tests
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
				if (!(instruction is TInstruction))
				{
					Debug.WriteLine("({0}) cannot be cast to {1} at index {2}".ToFormat(instruction, typeof(TInstruction).Name, _index));
					
					var map = new StringBuilder();
					map.AppendLine("Dumping instruction set:");
					for (var i = 0; i < _instructions.Length; ++i)
						map.AppendLine("\t{0}: {1} ({2})".ToFormat(i, _instructions[i].GetType().Name, _instructions[i]));

					Debug.WriteLine(map);
				}

				_index++;
				return instruction.As<TInstruction>();
			}

			public void Is<TInstruction>() where TInstruction : IModelMapInstruction
			{
				if (!(_instructions[_index] is TInstruction))
				{
					Debug.WriteLine("({0}) cannot be cast to {1} at index {2}".ToFormat(_instructions[_index], typeof(TInstruction).Name, _index));

					var map = new StringBuilder();
					map.AppendLine("Dumping instruction set:");
					for (var i = 0; i < _instructions.Length; ++i)
						map.AppendLine("\t{0}: {1} ({2})".ToFormat(i, _instructions[i].GetType().Name, _instructions[i]));

					Debug.WriteLine(map);
				}

				_instructions[_index].IsType<TInstruction>();
				_index++;
			}

			public void Verify<TInstruction>(Action<TInstruction> action) where TInstruction : IModelMapInstruction
			{
				action(Get<TInstruction>());
			}

			public void SkipDefaults()
			{
				Skip(18);
			}

			public void Skip(int length)
			{
				_index += length;
			}
		}
	}
}