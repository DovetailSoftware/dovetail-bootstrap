using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.ObjectModel;
using Dovetail.SDK.ModelMap.Transforms;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class HistoryModelMapVisitor : IHistoryModelMapVisitor
	{
		private readonly IMappingTransformRegistry _registry;
		private readonly IServiceLocator _services;
		private readonly IMappingVariableExpander _expander;
		private readonly ClarifyGeneric _rootGeneric;
		private readonly WorkflowObject _workflowObject;

		private readonly Stack<ModelInformation> _modelStack = new Stack<ModelInformation>();
		private readonly Stack<ClarifyGenericMapEntry> _genericStack = new Stack<ClarifyGenericMapEntry>();
		private readonly IList<ITransformArgument> _arguments = new List<ITransformArgument>();
		private readonly IList<BeginActEntry> _configurations = new List<BeginActEntry>();

		private FieldMap _currentFieldMap;
		private PropertyDefinition _propertyDef;
		private IMappingTransform _transform;
		private bool _ignoreInstructions;

		public HistoryModelMapVisitor(IMappingTransformRegistry registry, 
			IServiceLocator services,
			IMappingVariableExpander expander, 
			ClarifyGeneric rootGeneric, 
			ClarifyDataSet dataSet, 
			WorkflowObject workflowObject)
		{
			_registry = registry;
			_services = services;
			_expander = expander;
			_rootGeneric = rootGeneric;
			_workflowObject = workflowObject;

			DataSet = dataSet;
		}

		// public for testing
		public IEnumerable<ModelInformation> ModelStack
		{
			get { return _modelStack.ToArray(); }
		}

		// public for testing
		public IEnumerable<ClarifyGenericMapEntry> GenericStack
		{
			get { return _genericStack.ToArray(); }
		}

		public ClarifyGenericMapEntry RootGenericMap { get; private set; }

		// public for testing
		public ClarifyDataSet DataSet { get; private set; }

		// public for testing
		public IEnumerable<BeginActEntry> ActEntries
		{
			get { return _configurations; }
		}

		public void Visit(BeginModelMap instruction)
		{
			_modelStack.Push(new ModelInformation
			{
				ModelName = instruction.Name
			});

			var clarifyGenericMap = new ClarifyGenericMapEntry
			{
				ClarifyGeneric = _rootGeneric,
				Model = _modelStack.Peek()
			};

			_genericStack.Push(clarifyGenericMap);
		}

		public void Visit(BeginTable instruction)
		{
		}

		public void Visit(EndTable instruction)
		{
		}

		public void Visit(BeginView instruction)
		{
		}

		public void Visit(EndView instruction)
		{
		}

		public void Visit(BeginWhen instruction)
		{
			_ignoreInstructions = instruction.IsChild != _workflowObject.IsChild;
		}

		public void Visit(EndWhen instruction)
		{
			_ignoreInstructions = false;
		}

		public void Visit(EndModelMap instruction)
		{
			RootGenericMap = _genericStack.Peek();
		}

		public void Visit(BeginActEntry instruction)
		{
			executeInstruction(() =>
			{
				_configurations.Add(instruction);

				var begin = new BeginMappedProperty
				{
					Key = new DynamicValue("details"),
					AllowEmpty = true,
				};

				Visit(begin, _ => _.AsInt("act_code") == instruction.Code);
			});
		}

		public void Visit(EndActEntry instruction)
		{
			executeInstruction(() =>
			{
				Visit(new EndMappedProperty());
			});
		}

		public void Visit(BeginProperty instruction)
		{
			executeInstruction(() =>
			{
				var key = instruction.Key.Resolve(_services).ToString();
				_propertyDef = new PropertyDefinition
				{
					Key = instruction.Key.Resolve(_services).ToString()
				};

				if (instruction.Field == null)
				{
					return;
				}

				_currentFieldMap = new FieldMap
				{
					Key = key,
					FieldNames = new[] {instruction.Field.Resolve(_services).ToString()},
					IsIdentifier = instruction.IsIdentifier,
					PropertyType = PropertyTypes.Parse(instruction.DataType.Resolve(_services).ToString())
				};
			});
		}

		public void Visit(EndProperty instruction)
		{
			executeInstruction(() =>
			{
				_propertyDef = null;

				var currentGeneric = _genericStack.Peek();
				if (_currentFieldMap == null)
				{
					return;
				}

				currentGeneric.ClarifyGeneric.DataFields.AddRange(_currentFieldMap.FieldNames);

				if (currentGeneric.Model.ModelName != _modelStack.Peek().ModelName)
				{
					currentGeneric.Model.AddFieldMap(_currentFieldMap);
				}
				else
				{
					currentGeneric.AddFieldMap(_currentFieldMap);
				}

				_currentFieldMap = null;
			});
		}

		public void Visit(BeginAdHocRelation instruction)
		{
			executeInstruction(() =>
			{
				//validateAdhocRelation(beginAdHocRelation);

				var parentClarifyGenericMap = _genericStack.Peek();

				parentClarifyGenericMap.ClarifyGeneric.DataFields.Add(instruction.FromTableField.Resolve(_services).ToString());

				var tableGeneric =
					parentClarifyGenericMap.ClarifyGeneric.DataSet.CreateGeneric(instruction.ToTableName.Resolve(_services).ToString());
				tableGeneric.DataFields.Add(instruction.ToTableFieldName.Resolve(_services).ToString());

				var subRootInformation = new SubRootInformation
				{
					ParentKeyField = instruction.FromTableField.Resolve(_services).ToString(),
					RootKeyField = instruction.ToTableFieldName.Resolve(_services).ToString()
				};

				var model = _modelStack.Peek();
				var clarifyGenericMap = new ClarifyGenericMapEntry
				{
					ClarifyGeneric = tableGeneric,
					Model = new ModelInformation
					{
						ModelName = model.ModelName,
						ParentProperty = model.ParentProperty,
						IsCollection = model.IsCollection
					},
					NewRoot = subRootInformation
				};
				parentClarifyGenericMap.AddChildGenericMap(clarifyGenericMap);
				_genericStack.Push(clarifyGenericMap);
			});
		}

		public void Visit(BeginRelation instruction)
		{
			executeInstruction(() =>
			{
				var parentClarifyGenericMap = _genericStack.Peek();
				var relationGeneric =
					parentClarifyGenericMap.ClarifyGeneric.Traverse(instruction.RelationName.Resolve(_services).ToString());

				var model = _modelStack.Peek();
				var clarifyGenericMap = new ClarifyGenericMapEntry
				{
					ClarifyGeneric = relationGeneric,
					Model = new ModelInformation
					{
						ModelName = model.ModelName,
						ParentProperty = model.ParentProperty,
						IsCollection = model.IsCollection
					}
				};
				parentClarifyGenericMap.AddChildGenericMap(clarifyGenericMap);
				_genericStack.Push(clarifyGenericMap);
			});
		}

		public void Visit(EndRelation instruction)
		{
			executeInstruction(() =>
			{
				_genericStack.Pop();
			});
		}

		public void Visit(BeginMappedProperty instruction)
		{
			executeInstruction(() =>
			{
				Visit(instruction, null);
			});
		}

		public void Visit(BeginMappedProperty instruction, Func<ClarifyDataRow, bool> condition)
		{
			executeInstruction(() =>
			{
				var parentClarifyGenericMap = _genericStack.Peek();
				var key = instruction.Key.Resolve(_services).ToString();

				var childModel = new ModelInformation
				{
					ModelName = key,
					ParentProperty = key,
					AllowEmpty = instruction.AllowEmpty
				};

				var clarifyGenericMap = new ClarifyGenericMapEntry
				{
					ClarifyGeneric = parentClarifyGenericMap.ClarifyGeneric,
					Model = childModel,
					Condition = condition
				};

				parentClarifyGenericMap.AddChildGenericMap(clarifyGenericMap);
				_genericStack.Push(clarifyGenericMap);

				_modelStack.Push(childModel);
			});
		}

		public void Visit(EndMappedProperty instruction)
		{
			executeInstruction(() =>
			{
				_genericStack.Pop();
				_modelStack.Pop();
			});
		}

		public void Visit(BeginMappedCollection instruction)
		{
			executeInstruction(() =>
			{
				var key = instruction.Key.Resolve(_services).ToString();
				_modelStack.Push(new ModelInformation
				{
					ModelName = key,
					ParentProperty = key,
					IsCollection = true
				});
			});
		}

		public void Visit(EndMappedCollection instruction)
		{
			executeInstruction(() =>
			{
				_modelStack.Pop();
			});
		}

		public void Visit(FieldSortMap instruction)
		{
			executeInstruction(() =>
			{
				var currentGeneric = _genericStack.Peek();
				currentGeneric.ClarifyGeneric.AppendSort(instruction.Field.Resolve(_services).ToString(), instruction.IsAscending);
			});
		}

		public void Visit(AddFilter instruction)
		{
			executeInstruction(() =>
			{
				var currentGeneric = _genericStack.Peek();
				currentGeneric.ClarifyGeneric.Filter.AddFilter(instruction.Filter);
			});
		}

		public void Visit(BeginTransform instruction)
		{
			executeInstruction(() =>
			{
				var name = instruction.Name.Resolve(_services).ToString();
				if (!_registry.HasPolicy(name))
				{
					throw new ModelMapException("Invalid transform: \"{0}\"".ToFormat(instruction.Name));
				}

				_transform = (IMappingTransform) FastYetSimpleTypeActivator.CreateInstance(_registry.FindPolicy(name));
			});
		}

		public void Visit(AddTransformArgument instruction)
		{
			executeInstruction(() =>
			{
				if (instruction.Value != null)
				{
					_arguments.Add(new ValueArgument(instruction.Name.Resolve(_services).ToString(),
						instruction.Value.Resolve(_services)));
					return;
				}

				var field = instruction.Property.Resolve(_services).ToString();
				_arguments.Add(new FieldArgument(instruction.Name.Resolve(_services).ToString(), ModelDataPath.Parse(field)));
			});
		}

		public void Visit(RemoveProperty instruction)
		{
		}

		public void Visit(RemoveMappedProperty instruction)
		{
		}

		public void Visit(RemoveMappedCollection instruction)
		{
		}

		public void Visit(AddTag instruction)
		{
			executeInstruction(() =>
			{
				var generic = _genericStack.Peek();
				generic.AddTag(instruction.Tag);
			});
		}

		public void Visit(EndTransform instruction)
		{
			executeInstruction(() =>
			{
				var field = _propertyDef.Key;
				var path = ModelDataPath.Parse(field);
				var currentGeneric = _genericStack.Peek();

				currentGeneric.AddTransform(new ConfiguredTransform(path, _transform, _arguments.ToArray(), _expander, _services));

				_arguments.Clear();
			});
		}

		public void Visit(PushVariableContext instruction)
		{
			executeInstruction(() =>
			{
				_expander.PushContext(new VariableExpanderContext(null,
					instruction.Attributes.ToDictionary(_ => _.Key, _ => _.Value.Resolve(_services))));
			});
		}

		public void Visit(PopVariableContext instruction)
		{
			executeInstruction(() =>
			{
				_expander.PopContext();
			});
		}

		private void executeInstruction(Action action)
		{
			if (_ignoreInstructions) return;
			action();
		}
	}
}