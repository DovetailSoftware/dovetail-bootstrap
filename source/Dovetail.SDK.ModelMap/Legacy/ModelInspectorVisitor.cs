using System;
using System.Linq;
using System.Reflection;
using Dovetail.SDK.ModelMap.Legacy.Instructions;
using Dovetail.SDK.ModelMap.Legacy.ObjectModel;
using Dovetail.SDK.ModelMap.Legacy.Registration;
using FChoice.Foundation.Clarify.Schema;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Legacy
{
	public class ModelMapFieldDetails
	{
		/// <summary>
		/// Property of the Model class
		/// </summary>
		public PropertyInfo Property { get; set; }
		/// <summary>
		/// Name of the database field 
		/// </summary>
		public string FieldName { get; set; }

		/// <summary>
		/// Type of the field in the Clarify schema. Currently only string and integer types are supported.
		/// </summary>
		public Type SchemaFieldType { get; set; }
	}

	/// <summary>
	/// Inspects the model map for a model (generic argument). 
	/// Currently used to get at the identifying field details for a given model map. 
	/// </summary>
	/// <typeparam name="MODEL">Model whose ModelMap should be inspected.</typeparam>
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

		/// <summary>
		/// Inspects the model map and returns details about the identifying field of the map.
		/// </summary>
		/// <returns></returns>
		public ModelMapFieldDetails GetIdentifier()
		{
			if(!_visited)
            {
                _modelMap.Accept(_modelInspectorVisitor);
                _visited = true;
            }

			return _modelInspectorVisitor.IndentifierField;
		}
	}

	public class ModelInspectorVisitor : IModelMapVisitor
	{
	    private readonly ISchemaCache _schemaCache;
        private ISchemaTableBase _schemaTable;

	    public ModelInspectorVisitor(ISchemaCache schemaCache)
	    {
	        _schemaCache = schemaCache;
	    }

	    public ModelMapFieldDetails IndentifierField { get; set; }

		public void Visit(BeginModelMap beginModelMap)
		{
			IndentifierField = null;
		}

		public void Visit(FieldMap fieldMap)
		{
            if (!fieldMap.IsIdentifier) return;

            if(IndentifierField != null)
                throw new ArgumentException("There are multiple identifying fields defined on the model map being visited.");
            
		    IndentifierField = new ModelMapFieldDetails
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
	        
            var isString = schemaField.DataType == (int) SchemaCommonType.String;

            return isString ? typeof(string) : typeof(int);
	    }

	    public void Visit(BeginTable beginTable)
		{
		    _schemaTable = _schemaCache.Tables[beginTable.TableName];

			if(_schemaTable == null)
			{
				throw new Exception("No table {0} was found in the schema.".ToFormat(beginTable.TableName));
			}
		}

	    public void Visit(BeginView beginView)
	    {
	        _schemaTable = _schemaCache.Views[beginView.TableName];

			if (_schemaTable == null)
			{
				throw new Exception("No view {0} was found in the schema.".ToFormat(beginView.TableName));
			}
	    }
		public void Visit(BeginAdHocRelation beginAdHocRelation) { }
		public void Visit(BeginRelation beginRelation) { }
		public void Visit(EndRelation endRelation) { }
		public void Visit(BeginMapMany beginMapMany) { }
		public void Visit(EndMapMany endMapMany) { }
		public void Visit(FieldSortMap fieldSortMap ) { }
		public void Visit(AddFilter addFilter) { }
		public void Visit(EndModelMap endModelMap) { }
	}
}