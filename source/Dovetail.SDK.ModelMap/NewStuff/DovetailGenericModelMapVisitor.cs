using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using Dovetail.SDK.ModelMap.NewStuff.ObjectModel;
using Dovetail.SDK.ModelMap.NewStuff.Transforms;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
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

		public DovetailGenericModelMapVisitor(IClarifySession session, ISchemaCache schemaCache, IMappingTransformRegistry registry)
        {
            _session = session;
            _schemaCache = schemaCache;
			_registry = registry;
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
			_propertyDef = new PropertyDefinition
			{
				Key = instruction.Key
			};

			if (instruction.Field.IsEmpty())
	        {
		        return;
	        }

            _currentFieldMap = new FieldMap
            {
                Key = instruction.Key,
                FieldNames = new [] { instruction.Field },
                IsIdentifier = instruction.PropertyType == "identifier",
                PropertyType = PropertyTypes.Parse(instruction.DataType)
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
            currentGeneric.AddFieldMap(_currentFieldMap);
        }

        public void Visit(BeginAdHocRelation instruction)
        {
            //validateAdhocRelation(beginAdHocRelation);

            var parentClarifyGenericMap = _genericStack.Peek();

            parentClarifyGenericMap.ClarifyGeneric.DataFields.Add(instruction.FromTableField);

            var tableGeneric = parentClarifyGenericMap.ClarifyGeneric.DataSet.CreateGeneric(instruction.ToTableName);
            tableGeneric.DataFields.Add(instruction.ToTableFieldName);

            var subRootInformation = new SubRootInformation
            {
                ParentKeyField = instruction.FromTableField,
                RootKeyField = instruction.ToTableFieldName
            };

            var clarifyGenericMap = new ClarifyGenericMapEntry
            {
                ClarifyGeneric = tableGeneric,
                Model = _modelStack.Peek(),
                NewRoot = subRootInformation
            };
            parentClarifyGenericMap.AddChildGenericMap(clarifyGenericMap);
            _genericStack.Push(clarifyGenericMap);
        }

        public void Visit(BeginRelation instruction)
        {
            var parentClarifyGenericMap = _genericStack.Peek();
            var relationGeneric = parentClarifyGenericMap.ClarifyGeneric.Traverse(instruction.RelationName);

            var clarifyGenericMap = new ClarifyGenericMapEntry { ClarifyGeneric = relationGeneric, Model = _modelStack.Peek() };
            parentClarifyGenericMap.AddChildGenericMap(clarifyGenericMap);
            _genericStack.Push(clarifyGenericMap);
        }

        public void Visit(EndRelation instruction)
        {
            _genericStack.Pop();
        }

        public void Visit(BeginMappedProperty instruction)
        {
            _modelStack.Push(new ModelInformation
            {
                ModelName = instruction.Key,
                ParentProperty = instruction.Key
            });
        }

        public void Visit(EndMappedProperty instruction)
        {
            _modelStack.Pop();
        }

        public void Visit(BeginMappedCollection instruction)
        {
            _modelStack.Push(new ModelInformation
            {
                ModelName = instruction.Key,
                ParentProperty = instruction.Key,
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
            currentGeneric.ClarifyGeneric.AppendSort(instruction.Field, instruction.IsAscending);
        }

        public void Visit(AddFilter instruction)
        {
            var currentGeneric = _genericStack.Peek();
            currentGeneric.ClarifyGeneric.Filter.AddFilter(instruction.Filter);
        }

		public void Visit(BeginTransform instruction)
		{
			if (!_registry.HasPolicy(instruction.Name))
			{
				throw new ModelMapException("Invalid transform: \"{0}\"".ToFormat(instruction.Name));
			}

			_transform = (IMappingTransform) FastYetSimpleTypeActivator.CreateInstance(_registry.FindPolicy(instruction.Name));
		}

		public void Visit(AddTransformArgument instruction)
		{
			if (instruction.Value.IsNotEmpty())
			{
				_arguments.Add(new ValueArgument(instruction.Name, instruction.Value));
				return;
			}

			var field = instruction.Property;
			_arguments.Add(new FieldArgument(instruction.Name, ModelDataPath.Parse(field)));
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

			currentGeneric.AddTransform(new ConfiguredTransform(path, _transform, _arguments.ToArray()));

			_arguments.Clear();
		}
    }
}