using System;
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
			var model = new ModelMap.NewStuff.ModelMap("case", "case");
			model.AddInstruction(new BeginProperty { Key = new PassThruValue("caseId") });
			model.AddInstruction(new EndProperty());
			model.AddInstruction(new BeginRelation());
			model.AddInstruction(new IncludePartial
			{
				Name = "sitePartial",
				Attributes = new Dictionary<string, IDynamicValue>
				{
					{ "propertyName", new PassThruValue("siteId") }
				}
			});
			model.AddInstruction(new IncludePartial
			{
				Name = "reusablePartial",
				Attributes = new Dictionary<string, IDynamicValue>
				{
					{ "propertyName", new PassThruValue("myProperty") }
				}
			});
			model.AddInstruction(new EndRelation());
			model.AddInstruction(new BeginRelation());
			model.AddInstruction(new IncludePartial { Name = "addressPartial" });
			model.AddInstruction(new EndRelation());

			var p1 = new ModelMap.NewStuff.ModelMap("reusablePartial", null);
			p1.AddInstruction(new BeginProperty { Key = new PassThruValue("${propertyName}") });
			p1.AddInstruction(new EndProperty());

			var p2 = new ModelMap.NewStuff.ModelMap("sitePartial", null);
			p2.AddInstruction(new BeginProperty { Key = new PassThruValue("${propertyName}") });
			p2.AddInstruction(new EndProperty());
			p2.AddInstruction(new IncludePartial
			{
				Name = "reusablePartial",
				Attributes = new Dictionary<string, IDynamicValue>
				{
					{ "propertyName", new PassThruValue("myOtherProperty") }
				}
			});

			var p3 = new ModelMap.NewStuff.ModelMap("addressPartial", null);
			p3.AddInstruction(new BeginProperty { Key = new PassThruValue("addressId") });
			p3.AddInstruction(new EndProperty());

			var cache = new StubModelMapCache();
			cache.ThePartials.Add(p1);
			cache.ThePartials.Add(p2);
			cache.ThePartials.Add(p3);

			model.As<IExpandableMap>().Expand(cache);

			var instructions = new List<IModelMapInstruction>();
			var visitor = new RecordingVisitor(instructions);

			model.Accept(visitor);

			VerifyInstructions.Assert(instructions, _ =>
			{
				_.Verify<BeginProperty>(__ => __.Key.ToString().ShouldEqual("caseId"));
				_.Is<EndProperty>();

				_.Is<BeginRelation>();

				// sitePartial
				_.Verify<PushVariableContext>(__ => __.Attributes["propertyName"].ToString().ShouldEqual("siteId"));

				_.Verify<BeginProperty>(__ => __.Key.ToString().ShouldEqual("${propertyName}"));
				_.Is<EndProperty>();

				// sitePartial => reusablePartial
				_.Verify<PushVariableContext>(__ => __.Attributes["propertyName"].ToString().ShouldEqual("myOtherProperty"));
				_.Verify<BeginProperty>(__ => __.Key.ToString().ShouldEqual("${propertyName}"));
				_.Is<EndProperty>();
				_.Is<PopVariableContext>();

				_.Is<PopVariableContext>(); //end sitePartial

				// reusablePartial
				_.Verify<PushVariableContext>(__ => __.Attributes["propertyName"].ToString().ShouldEqual("myProperty"));
				_.Verify<BeginProperty>(__ => __.Key.ToString().ShouldEqual("${propertyName}"));
				_.Is<EndProperty>();
				_.Is<PopVariableContext>(); //end reusablePartial

				_.Is<EndRelation>();
				_.Is<BeginRelation>();

				// addressPartial
				_.Is<PushVariableContext>();
				_.Verify<BeginProperty>(__ => __.Key.ToString().ShouldEqual("addressId"));
				_.Is<EndProperty>();
				_.Is<PopVariableContext>(); //end addressPartial

				_.Is<EndRelation>();
			});
		}

		public class PassThruValue : IDynamicValue
		{
			private readonly object _value;

			public PassThruValue(object value)
			{
				_value = value;
			}

			public object Resolve(IServiceLocator services)
			{
				return _value;
			}

			public override string ToString()
			{
				return _value.ToString();
			}
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

			public void Visit(RemoveProperty instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(RemoveMappedProperty instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(RemoveMappedCollection instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(AddTag instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(PushVariableContext instruction)
			{
				_instructions.Add(instruction);
			}

			public void Visit(PopVariableContext instruction)
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