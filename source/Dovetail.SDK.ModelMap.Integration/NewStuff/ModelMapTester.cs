using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FubuCore;
using NUnit.Framework;
using BeginRelation = Dovetail.SDK.ModelMap.NewStuff.Instructions.BeginRelation;
using EndRelation = Dovetail.SDK.ModelMap.NewStuff.Instructions.EndRelation;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff
{
	[TestFixture]
	public class ModelMapTester
	{
		[Test]
		public void expands_the_partial()
		{
			var model = new ModelMap.NewStuff.ModelMap("case");
			model.AddInstruction(new BeginProperty { Key = "caseId" });
			model.AddInstruction(new EndProperty());
			model.AddInstruction(new BeginRelation());
			model.AddInstruction(new IncludePartial
			{
				Name = "sitePartial",
				Attributes = new Dictionary<string, string>
				{
					{ "propertyName", "siteId" }
				}
			});
			model.AddInstruction(new EndRelation());
			model.AddInstruction(new BeginRelation());
			model.AddInstruction(new IncludePartial { Name = "addressPartial" });
			model.AddInstruction(new EndRelation());

			var p1 = new ModelMap.NewStuff.ModelMap("sitePartial");
			p1.AddInstruction(new BeginProperty { Key = "${propertyName}" });
			p1.AddInstruction(new EndProperty());

			var p2 = new ModelMap.NewStuff.ModelMap("addressPartial");
			p2.AddInstruction(new BeginProperty { Key = "addressId" });
			p2.AddInstruction(new EndProperty());

			var cache = new StubModelMapCache();
			cache.ThePartials.Add(p1);
			cache.ThePartials.Add(p2);

			model.As<IExpandableMap>().Expand(cache);

			var instructions = new List<IModelMapInstruction>();
			var visitor = new RecordingVisitor(instructions);

			model.Accept(visitor);

			instructions[0].As<BeginProperty>().Key.ShouldEqual("caseId");
			instructions[1].ShouldBeOfType<EndProperty>();
			instructions[2].ShouldBeOfType<BeginRelation>();
			instructions[3].As<BeginProperty>().Key.ShouldEqual("siteId");
			instructions[4].ShouldBeOfType<EndProperty>();
			instructions[5].ShouldBeOfType<EndRelation>();
			instructions[6].ShouldBeOfType<BeginRelation>();
			instructions[7].As<BeginProperty>().Key.ShouldEqual("addressId");
			instructions[8].ShouldBeOfType<EndProperty>();
			instructions[9].ShouldBeOfType<EndRelation>();
		}

		private class RecordingVisitor : ModelMap.NewStuff.IModelMapVisitor
		{
			private readonly IList<IModelMapInstruction> _instructions;

			public RecordingVisitor(IList<IModelMapInstruction> instructions)
			{
				_instructions = instructions;
			}

			public void Visit(ModelMap.NewStuff.Instructions.BeginModelMap instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(ModelMap.NewStuff.Instructions.BeginTable instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(EndTable instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(ModelMap.NewStuff.Instructions.BeginView instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(EndView instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(ModelMap.NewStuff.Instructions.EndModelMap instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(BeginProperty instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(EndProperty instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(ModelMap.NewStuff.Instructions.BeginAdHocRelation instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(BeginRelation instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(EndRelation instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(BeginMappedProperty instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(EndMappedProperty instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(BeginMappedCollection instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(EndMappedCollection instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(ModelMap.NewStuff.Instructions.FieldSortMap instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(ModelMap.NewStuff.Instructions.AddFilter instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(BeginTransform instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(EndTransform instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(AddTransformArgument instruction)
			{
				_instructions.Add(instruction);
			}
		}

		private class StubModelMapCache : IModelMapCache
		{
			public List<ModelMap.NewStuff.ModelMap> TheMaps = new List<ModelMap.NewStuff.ModelMap>();
			public List<ModelMap.NewStuff.ModelMap> ThePartials = new List<ModelMap.NewStuff.ModelMap>();

			public IEnumerable<ModelMap.NewStuff.ModelMap> Maps()
			{
				return TheMaps;
			}

			public IEnumerable<ModelMap.NewStuff.ModelMap> Partials()
			{
				return ThePartials;
			}

			public void Clear()
			{
			}
		}
	}
}