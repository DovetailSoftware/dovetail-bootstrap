using System;
using System.Linq;
using System.Reflection;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.ObjectModel;
using Dovetail.SDK.ModelMap.Registration;
using FChoice.Foundation.Clarify.Schema;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.ModelMap
{
	public class ModelInspector<MODEL>
	{
	    private readonly ModelMap<MODEL> _modelMap;
	    private readonly ModelInspectorVisitor _modelInspectorVisitor;
	    private bool _visited;

		public ModelInspector(ModelMap<MODEL> modelMap, ModelInspectorVisitor modelInspectorVisitor)
		{
		    _modelMap = modelMap;
		    _modelInspectorVisitor = modelInspectorVisitor;
		}

	    public MapFieldDetails Identifier
		{
			get
			{
                if(!_visited)
                {
                    _modelMap.Accept(_modelInspectorVisitor);
                    _visited = true;
                }

			    return _modelInspectorVisitor.IndentifierField;
			}
		}
	}

	public class MapFieldDetails
	{
		public PropertyInfo Property { get; set; }
		public string FieldName { get; set; }
	    public Type SchemaFieldType { get; set; }
	}

	public class ModelInspectorVisitor : IModelMapVisitor
	{
	    private readonly ISchemaCache _schemaCache;
        private ISchemaTableBase _schemaTable;

	    public ModelInspectorVisitor(ISchemaCache schemaCache)
	    {
	        _schemaCache = schemaCache;
	    }

	    public MapFieldDetails IndentifierField { get; set; }

		public void Visit(FieldMap fieldMap)
		{
            if (!fieldMap.IsIdentifier) return;

            if(IndentifierField != null)
                throw new ArgumentException("There are multiple identifying fields defined on the model map being visited.");
            
		    IndentifierField = new MapFieldDetails
		                           {
		                               FieldName = fieldMap.FieldNames.First(),
		                               Property = fieldMap.Property,
                                       SchemaFieldType = getSchemaFieldType(fieldMap)
		                           };
		}

	    private Type getSchemaFieldType(FieldMap fieldMap)
	    {
            var identifyingFieldName = fieldMap.FieldNames.First();
	        _schemaCache.IsValidField(_schemaTable.Name, identifyingFieldName);
	        
	        var schemaField = _schemaTable.Fields[identifyingFieldName];
	        
            
            var isString = (int) schemaField.DataType == (int) SchemaCommonType.String;

            return isString ? typeof(string) : typeof(int);
	    }

	    public void Visit(BeginTable beginTable)
		{
		    _schemaTable = _schemaCache.Tables[beginTable.TableName];
		}

	    public void Visit(BeginView beginView)
	    {
	        _schemaTable = _schemaCache.Views[beginView.TableName];
	    }
		public void Visit(BeginAdHocRelation beginAdHocRelation) { }
		public void Visit(BeginRelation beginRelation) { }
		public void Visit(EndRelation endRelation) { }
		public void Visit(BeginMapMany beginMapMany) { }
		public void Visit(EndMapMany endMapMany) { }
		public void Visit(FieldSortMap fieldSortMap ) { }
		public void Visit(AddFilter addFilter) { }
		public void Visit(BeginModelMap beginModelMap) { }
		public void Visit(EndModelMap endModelMap) { }
	}
}