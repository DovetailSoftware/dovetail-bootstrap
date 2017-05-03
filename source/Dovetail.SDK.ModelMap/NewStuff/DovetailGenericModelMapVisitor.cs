using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using Dovetail.SDK.ModelMap.NewStuff.ObjectModel;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class DovetailGenericModelMapVisitor : IModelMapVisitor
    {
        private readonly IClarifySession _session;
        private readonly ISchemaCache _schemaCache;
        private readonly Stack<ModelInformation> _modelStack = new Stack<ModelInformation>();
        private readonly Stack<ClarifyGenericMapEntry> _genericStack = new Stack<ClarifyGenericMapEntry>();

        public DovetailGenericModelMapVisitor(IClarifySession session, ISchemaCache schemaCache)
        {
            _session = session;
            _schemaCache = schemaCache;
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

        private FieldMap _currentFieldMap;

        public void Visit(BeginProperty instruction)
        {
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
            var currentGeneric = _genericStack.Peek();
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

            var clarifyGenericMap = new ClarifyGenericMapEntry { ClarifyGeneric = relationGeneric, Model = _modelStack.Peek(), Relation = instruction.RelationName };
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
    }
}