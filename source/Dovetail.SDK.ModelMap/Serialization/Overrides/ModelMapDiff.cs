using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Serialization.Overrides
{
	public class ModelMapDiff : IModelMapDiff
	{
		public void Diff(ModelMap map, ModelMap overrides, ModelMapDiffOptions options)
		{
			removeProperties(map, overrides, options);
			addProperties(map, overrides, options);
		}

		private void removeProperties(ModelMap map, ModelMap overrides, ModelMapDiffOptions options)
		{
			options.Removals.Each(_ => executeRemoveInstruction(map, overrides, options, _));

			var prunedInstructions = new List<IModelMapInstruction>();
			var contexts = new Stack<IModelMapInstruction>();
			foreach (var instruction in map.Instructions)
			{
				if (contexts.Count != 0)
				{
					var previous = contexts.Peek();
					var previousType = previous.GetType();

					if ((previousType == typeof(BeginRelation) && instruction.GetType() == typeof(EndRelation))
					    || (previousType == typeof(BeginAdHocRelation) && instruction.GetType() == typeof(EndRelation)))
					{
						prunedInstructions.Add(previous);
						prunedInstructions.Add(instruction);
						contexts.Pop();
						continue;
					}
				}

				contexts.Push(instruction);
			}

			prunedInstructions.Each(map.RemoveInstruction);
		}

		private void executeRemoveInstruction(ModelMap map, ModelMap overrides, ModelMapDiffOptions options, ConfiguredRemoval remove)
		{
			var targetIndex = 0;
			var mapInstructions = map.Instructions.ToList();
			var sets = new List<InstructionSet>();
			foreach (var instruction in overrides.Instructions)
			{
				if (shouldOffset(instruction, options))
				{
					var i = mapInstructions.IndexOf(instruction);
					if (i != -1)
					{
						targetIndex = i;
					}
					else
					{
						targetIndex += 1;
					}
				}

				if (instruction.GetType().CanBeCastTo(remove.InstructionType))
				{
					var removal = instruction as IModelMapRemovalInstruction;
					if (removal != null)
					{
						var set = remove.Find(map, removal.Key, targetIndex);
						sets.Add(set);
					}
				}
			}

			var offset = 0;
			foreach (var set in sets)
			{
				map.Remove(set, offset);
				offset += set.Instructions.Count();
			}
		}

		private void addProperties(ModelMap map, ModelMap overrides, ModelMapDiffOptions options)
		{
			var targetIndex = -1;
			var instructionsToAdd = new List<IModelMapInstruction>();
			var contexts = new Stack<IModelMapInstruction>();
			foreach (var instruction in overrides.Instructions.Where(_ => _.GetType() != typeof(Instructions.RemoveProperty)).ToArray())
			{
				var mapInstructions = map.Instructions.ToList();
				if (shouldOffset(instruction, options))
				{
					var i = mapInstructions.IndexOf(instruction);
					if (i != -1)
					{
						targetIndex = i + 1;
					}
				}

				if (targetIndex == -1)
					continue;

				var context = options.PropertyContexts.SingleOrDefault(_ => _.Matches(instruction.GetType()));
				if (context != null && mapInstructions.IndexOf(instruction) == -1)
				{
					contexts.Push(context.WaitFor());
				}

				if (contexts.Count != 0)
					instructionsToAdd.Add(instruction);

				if (contexts.Count != 0 && contexts.Peek().GetType() == instruction.GetType())
					contexts.Pop();

				if (contexts.Count == 0 && instructionsToAdd.Count != 0)
				{
					map.InsertInstructions(targetIndex, instructionsToAdd);
					targetIndex += instructionsToAdd.Count;
					instructionsToAdd.Clear();
				}
			}
		}

		private static bool shouldOffset(IModelMapInstruction instruction, ModelMapDiffOptions options)
		{
			return options.Offsets.Contains(instruction.GetType());
		}
	}
}