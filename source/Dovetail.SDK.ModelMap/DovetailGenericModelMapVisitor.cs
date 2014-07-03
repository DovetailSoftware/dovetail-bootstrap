using System;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.ObjectModel;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.ModelMap
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

		public void Visit(BeginModelMap beginModelMap)
		{
			DataSet = _session.CreateDataSet();

			_modelStack.Push(new ModelInformation {ModelType = beginModelMap.ModelType});
		}

		public void Visit(BeginTable beginTable)
		{
			var rootGeneric = DataSet.CreateGeneric(beginTable.TableName);

			var clarifyGenericMap = new ClarifyGenericMapEntry {ClarifyGeneric = rootGeneric, Model = _modelStack.Peek()};
			_genericStack.Push(clarifyGenericMap);
		}

		public void Visit(BeginView beginView)
		{
			var rootGeneric = DataSet.CreateGeneric(beginView.TableName);

			var clarifyGenericMap = new ClarifyGenericMapEntry {ClarifyGeneric = rootGeneric, Model = _modelStack.Peek()};
			_genericStack.Push(clarifyGenericMap);
		}

		public void Visit(BeginAdHocRelation beginAdHocRelation)
		{
			validateAdhocRelation(beginAdHocRelation);

			var parentClarifyGenericMap = _genericStack.Peek();

			parentClarifyGenericMap.ClarifyGeneric.DataFields.Add(beginAdHocRelation.FromTableField);

			var tableGeneric = parentClarifyGenericMap.ClarifyGeneric.DataSet.CreateGeneric(beginAdHocRelation.ToTableName);
			tableGeneric.DataFields.Add(beginAdHocRelation.ToTableFieldName);

			var subRootInformation = new SubRootInformation
			{
				ParentKeyField = beginAdHocRelation.FromTableField,
				RootKeyField = beginAdHocRelation.ToTableFieldName
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

		private void validateAdhocRelation(BeginAdHocRelation beginAdHocRelation)
		{
			var currentTableName = getCurrentTableName();

			if (!_schemaCache.IsValidField(currentTableName, beginAdHocRelation.FromTableField))
				throw new ApplicationException("Cannot create an AdHocRelation from table {0} using invalid field {1}."
					.ToFormat(currentTableName, beginAdHocRelation.FromTableField));

			if (!_schemaCache.IsValidTableOrView(beginAdHocRelation.ToTableName))
				throw new ApplicationException("Cannot create an AdHocRelation from table {0} to invalid table {1}."
					.ToFormat(currentTableName, beginAdHocRelation.ToTableName));

			if (!_schemaCache.IsValidField(beginAdHocRelation.ToTableName, beginAdHocRelation.ToTableFieldName))
				throw new ApplicationException("Cannot create an AdHocRelation from table {0} to table {1} with invalid field {2}."
					.ToFormat(currentTableName, beginAdHocRelation.ToTableName, beginAdHocRelation.ToTableFieldName));
		}

		private string getCurrentTableName()
		{
			return _genericStack.Peek().ClarifyGeneric.TableName;
		}

		public void Visit(BeginRelation beginRelation)
		{
			var parentClarifyGenericMap = _genericStack.Peek();
			var relationGeneric = parentClarifyGenericMap.ClarifyGeneric.Traverse(beginRelation.RelationName);

			var clarifyGenericMap = new ClarifyGenericMapEntry {ClarifyGeneric = relationGeneric, Model = _modelStack.Peek()};
			parentClarifyGenericMap.AddChildGenericMap(clarifyGenericMap);
			_genericStack.Push(clarifyGenericMap);
		}

		public void Visit(EndRelation endRelation)
		{
			_genericStack.Pop();
		}

		public void Visit(BeginMapMany beginMapMany)
		{
			// TODO -- verify parent Model type matches top of stack?
			_modelStack.Push(new ModelInformation
			{
				ModelType = beginMapMany.ChildModelType,
				ParentProperty = beginMapMany.MappedProperty
			});
		}

		public void Visit(EndMapMany endMapMany)
		{
			_modelStack.Pop();
		}

		public void Visit(FieldMap fieldMap)
		{
			//if (fieldMap.FieldValueMethod != null)
			//{
			//    var badFields = verifyFields(fieldMap.FieldNames);
			//    if (badFields.Any())
			//    {
			//        throw new ApplicationException("Could not {0} : fields {1} were not valid.".ToFormat(fieldMap, String.Join(",", badFields.ToArray())));
			//    }
			//}

			var currentGeneric = _genericStack.Peek();
			currentGeneric.ClarifyGeneric.DataFields.AddRange(fieldMap.FieldNames);
			currentGeneric.AddFieldMap(fieldMap);
		}

		//private IEnumerable<string> verifyFields(string[] fieldNames)
		//{
		//    if (fieldNames == null || !fieldNames.Any())
		//        return new string[0];

		//    var tableName = getCurrentTableName();

		//    return fieldNames.Where(fieldName => fieldName.IsEmpty() || !_schemaCache.IsValidField(tableName, fieldName));
		//}

		public void Visit(EndModelMap endModelMap)
		{
			RootGenericMap = _genericStack.Peek();
		}

		public void Visit(FieldSortMap fieldSortMap)
		{
			//verifyFields(new[] { fieldSortMap.SchemaField });

			var currentGeneric = _genericStack.Peek();
			currentGeneric.ClarifyGeneric.AppendSort(fieldSortMap.FieldName, fieldSortMap.IsAscending);
		}

		public void Visit(AddFilter addFilter)
		{
			var currentGeneric = _genericStack.Peek();
			currentGeneric.ClarifyGeneric.Filter.AddFilter(addFilter.Filter);
		}
	}
}