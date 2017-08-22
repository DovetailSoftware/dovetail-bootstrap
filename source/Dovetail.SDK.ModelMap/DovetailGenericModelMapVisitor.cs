using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.ObjectModel;
using Dovetail.SDK.ModelMap.Transforms;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
	public class DovetailGenericModelMapVisitor : IModelMapVisitor
    {
        private readonly IClarifySession _session;
        private readonly ISchemaCache _schemaCache;
		private readonly IMappingTransformRegistry _registry;
        private readonly Stack<ModelInformation> _modelStack = new Stack<ModelInformation>();
        private readonly Stack<ClarifyGenericMapEntry> _genericStack = new Stack<ClarifyGenericMapEntry>();
		private readonly IList<ITransformArgument> _arguments = new List<ITransformArgument>();

		private FieldMap _currentFieldMap;
		private PropertyDefinition _propertyDef;
		private IMappingTransform _transform;
		private readonly IServiceLocator _services;
		private readonly IMappingVariableExpander _expander;

		public DovetailGenericModelMapVisitor(IClarifySession session, ISchemaCache schemaCache, IMappingTransformRegistry registry, IServiceLocator services, IMappingVariableExpander expander)
        {
            _session = session;
            _schemaCache = schemaCache;
			_registry = registry;
			_services = services;
			_expander = expander;
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

        public void Visit(BeginModelMap instruction)
        {
            DataSet = _session.CreateDataSet();

            _modelStack.Push(new ModelInformation { ModelName = instruction.Name });
        }

        public void Visit(BeginTable instruction)
        {
            var rootGeneric = DataSet.CreateGeneric(instruction.TableName);

            var clarifyGenericMap = new ClarifyGenericMapEntry { ClarifyGeneric = rootGeneric, Model = _modelStack.Peek() };
            _genericStack.Push(clarifyGenericMap);
        }

        public void Visit(EndTable instruction)
        {
        }

        public void Visit(BeginView instruction)
        {
            var rootGeneric = DataSet.CreateGeneric(instruction.ViewName);

            var clarifyGenericMap = new ClarifyGenericMapEntry { ClarifyGeneric = rootGeneric, Model = _modelStack.Peek() };
            _genericStack.Push(clarifyGenericMap);
        }

        public void Visit(EndView instruction)
        {
        }

        public void Visit(EndModelMap instruction)
        {
            RootGenericMap = _genericStack.Peek();
        }

        public void Visit(BeginProperty instruction)
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
                FieldNames = new [] { instruction.Field.Resolve(_services).ToString() },
                IsIdentifier = instruction.IsIdentifier,
                PropertyType = PropertyTypes.Parse(instruction.DataType.Resolve(_services).ToString())
            };
        }

        public void Visit(EndProperty instruction)
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
        }

        public void Visit(BeginAdHocRelation instruction)
        {
            //validateAdhocRelation(beginAdHocRelation);

            var parentClarifyGenericMap = _genericStack.Peek();

            parentClarifyGenericMap.ClarifyGeneric.DataFields.Add(instruction.FromTableField.Resolve(_services).ToString());

            var tableGeneric = parentClarifyGenericMap.ClarifyGeneric.DataSet.CreateGeneric(instruction.ToTableName.Resolve(_services).ToString());
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
        }

        public void Visit(BeginRelation instruction)
        {
            var parentClarifyGenericMap = _genericStack.Peek();
            var relationGeneric = parentClarifyGenericMap.ClarifyGeneric.Traverse(instruction.RelationName.Resolve(_services).ToString());

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
        }

        public void Visit(EndRelation instruction)
        {
            _genericStack.Pop();
        }

        public void Visit(BeginMappedProperty instruction)
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
				Model = childModel
			};

			parentClarifyGenericMap.AddChildGenericMap(clarifyGenericMap);
			_genericStack.Push(clarifyGenericMap);

			_modelStack.Push(childModel);
        }

        public void Visit(EndMappedProperty instruction)
        {
	        _genericStack.Pop();
            _modelStack.Pop();
        }

        public void Visit(BeginMappedCollection instruction)
        {
	        var key = instruction.Key.Resolve(_services).ToString();
			_modelStack.Push(new ModelInformation
            {
                ModelName = key,
                ParentProperty = key,
                IsCollection = true
            });
        }

        public void Visit(EndMappedCollection instruction)
        {
            _modelStack.Pop();
        }

        public void Visit(FieldSortMap instruction)
        {
            var currentGeneric = _genericStack.Peek();
            currentGeneric.ClarifyGeneric.AppendSort(instruction.Field.Resolve(_services).ToString(), instruction.IsAscending);
        }

        public void Visit(AddFilter instruction)
        {
            var currentGeneric = _genericStack.Peek();
            currentGeneric.ClarifyGeneric.Filter.AddFilter(instruction.Filter);
        }

		public void Visit(BeginTransform instruction)
		{
			var name = instruction.Name.Resolve(_services).ToString();
			if (!_registry.HasPolicy(name))
			{
				throw new ModelMapException("Invalid transform: \"{0}\"".ToFormat(instruction.Name));
			}

			_transform = (IMappingTransform) FastYetSimpleTypeActivator.CreateInstance(_registry.FindPolicy(name));
		}

		public void Visit(AddTransformArgument instruction)
		{
			if (instruction.Value != null)
			{
				_arguments.Add(new ValueArgument(instruction.Name.Resolve(_services).ToString(), instruction.Value.Resolve(_services)));
				return;
			}

			var field = instruction.Property.Resolve(_services).ToString();
			_arguments.Add(new FieldArgument(instruction.Name.Resolve(_services).ToString(), ModelDataPath.Parse(field)));
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
			var generic = _genericStack.Peek();
			generic.AddTag(instruction.Tag);
		}

		public void Visit(EndTransform instruction)
		{
			var field = _propertyDef.Key;
			var path = ModelDataPath.Parse(field);
			var currentGeneric = _genericStack.Peek();

			currentGeneric.AddTransform(new ConfiguredTransform(path, _transform, _arguments.ToArray(), _expander, _services));

			_arguments.Clear();
		}

		public void Visit(PushVariableContext instruction)
		{
			_expander.PushContext(new VariableExpanderContext(null, instruction.Attributes.ToDictionary(_ => _.Key, _ => _.Value.Resolve(_services))));
		}

		public void Visit(PopVariableContext instruction)
		{
			_expander.PopContext();
		}
	}
}