using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization.Overrides
{
	public class ModelMapDiff : IModelMapDiff
	{
		public void Diff(ModelMap map, ModelMap overrides)
		{
			removeProperties(map, overrides);	
		}

		private void removeProperties(ModelMap map, ModelMap overrides)
		{
			var removals = overrides.Instructions.OfType<Instructions.RemoveProperty>().ToArray();
			foreach (var removal in removals)
			{
				var instructionsToRemove = new List<IModelMapInstruction>();
				int count = 0;
				var removing = false;
				foreach (var instruction in map.Instructions)
				{
					var beginProp = instruction as BeginProperty;
					if (beginProp != null && beginProp.Key.EqualsIgnoreCase(removal.Key))
					{
						removing = true;
					}

					if (removing)
					{
						instructionsToRemove.Add(instruction);

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
							break;
					}
				}

				instructionsToRemove.Each(map.RemoveInstruction);
			}
		}
	}
}