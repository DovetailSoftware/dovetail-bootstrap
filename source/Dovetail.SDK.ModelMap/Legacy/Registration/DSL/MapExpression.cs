using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dovetail.SDK.ModelMap.Clarify;
using Dovetail.SDK.ModelMap.Legacy.Instructions;
using Dovetail.SDK.ModelMap.Legacy.ObjectModel;
using FChoice.Foundation.Filters;
using FubuCore;
using FubuCore.Reflection;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
	public class MapExpression<MODEL>
        : IMapExpressionFromRoot<MODEL>, IMapExpressionPostRoot<MODEL>,
            IMapExpressionPostAssign<MODEL>, IMapExpressionPostBasedOnField<MODEL>,
			IMapExpressionPostAssignWithList<MODEL>, IMapExpressionPostBasedOnFields<MODEL>, IMapExpressionPostDoNotEncode<MODEL>
    {
        private readonly IModelMapVisitor _mapVisitor;
        private FieldMap _currentFieldMap;

        public MapExpression(IModelMapVisitor mapVisitor)
        {
            _mapVisitor = mapVisitor;
        }

        public IModelMapVisitor MapVisitor
        {
            get { return _mapVisitor; }
        }

        public IMapExpressionPostRoot<MODEL> FromTable(string tableName)
        {
            MapVisitor.Visit(new BeginTable { TableName = tableName });

            return this;
        }

        public IMapExpressionPostView<MODEL> FromView(string viewName)
        {
            MapVisitor.Visit(new BeginView { TableName = viewName });

            return this;
        }

        public IMapExpressionPostRoot<MODEL> SortAscendingBy(string fieldName)
        {
            MapVisitor.Visit(new FieldSortMap(fieldName, true));

            return this;
        }

        public IMapExpressionPostRoot<MODEL> SortDescendingBy(string fieldName)
        {
            MapVisitor.Visit(new FieldSortMap(fieldName, false));

            return this;
        }

		public IMapExpressionPostRoot<MODEL> FilteredBy(Func<FilterExpression, Filter> filterAction)
		{
			var filter = filterAction(new FilterExpression());

			MapVisitor.Visit(new AddFilter(filter));

			return this;
		}

		public IMapExpressionPostRoot<MODEL> FilteredBy(Filter filter)
		{
			MapVisitor.Visit(new AddFilter(filter));

			return this;
		}

		public virtual IMapExpressionPostRoot<MODEL> ViaRelation(string relationName, Action<IMapExpressionPostRoot<MODEL>> mapAction)
        {
            var childMapExpression = new MapExpression<MODEL>(MapVisitor);

            MapVisitor.Visit(new BeginRelation { RelationName = relationName });

            mapAction(childMapExpression);

            MapVisitor.Visit(new EndRelation());

            return this;
        }

        public IMapExpressionPostRoot<MODEL> ViaAdhocRelation(string fromFieldName, string toTableName, string toFieldName, Action<IMapExpressionPostRoot<MODEL>> mapAction)
        {
            var childMapExpression = new MapExpression<MODEL>(MapVisitor);

            MapVisitor.Visit(new BeginAdHocRelation { ToTableFieldName = toFieldName, FromTableField = fromFieldName, ToTableName = toTableName });

            mapAction(childMapExpression);

            MapVisitor.Visit(new EndRelation());

            return this;
        }

        public IMapRelatedModelExpression<MODEL, CHILDDTO> MapMany<CHILDDTO>()
        {
            return new MapRelatedModelExpression<MODEL, CHILDDTO>(this, MapVisitor);
        }

        public IMapRelatedModelExpression<MODEL, CHILDDTO> MapOne<CHILDDTO>()
        {
            return new MapRelatedModelExpression<MODEL, CHILDDTO>(this, MapVisitor);
        }

        public IMapExpressionPostAssign<MODEL> Assign(Expression<Func<MODEL, object>> expression)
        {
            verifyFieldExpressionIsNotBeingConfigured();

            var propertyInfo = ReflectionHelper.GetProperty(expression);
            _currentFieldMap = new FieldMap { ModelType = typeof(MODEL), Property = propertyInfo };

            return this;
        }

        public IMapExpressionPostAssignWithList<MODEL> Assign(Expression<Func<MODEL, IClarifyList>> expression)
        {
            verifyFieldExpressionIsNotBeingConfigured();

            var propertyInfo = ReflectionHelper.GetProperty(expression);
            _currentFieldMap = new FieldMap { ModelType = typeof(MODEL), Property = propertyInfo };

            return this;
        }

		//TODO mark this method obsolete
        public IMapExpressionPostRoot<MODEL> FromGlobalList(string globalListName)
        {
			return FromGlobalList(globalListName, config => { });
        }

		//TODO mark this method obsolete
		public IMapExpressionPostRoot<MODEL> FromGlobalList(string globalListName, string selectionFromRelationNamed)
        {
        	return FromGlobalList(globalListName, config => config.SelectionFromRelation = selectionFromRelationNamed);
        }

		//TODO mark this method obsolete
		public IMapExpressionPostRoot<MODEL> FromGlobalList(string globalListName, Action<GlobalListConfigurationExpression> config)
		{
			verifyFieldExpressionIsBeingConfigured();
			
			var globalListConfigurationExpression = new GlobalListConfigurationExpression(globalListName);
			
			config(globalListConfigurationExpression);

			_currentFieldMap.GlobalListConfig = globalListConfigurationExpression;

			var fields = new List<string>();

			if (globalListConfigurationExpression.SelectionFromRelation.IsNotEmpty())
				fields.Add(globalListConfigurationExpression.SelectionFromRelation);

			return FromFields(fields.ToArray());
		}

		//TODO mark this method obsolete
		public IMapExpressionPostRoot<MODEL> FromUserDefinedList(string listName, string[] listValues)
        {
            verifyFieldExpressionIsBeingConfigured();

            _currentFieldMap.UserDefinedList = new UserDefinedList(listName, listValues);

            MapVisitor.Visit(_currentFieldMap);

            _currentFieldMap = null;

            return this;
        }


        public IMapExpressionPostRoot<MODEL> Do(Func<string, object> mapFieldValueToObject)
        {
            _currentFieldMap.StringToFieldValueMethod = mapFieldValueToObject;

            MapVisitor.Visit(_currentFieldMap);

            _currentFieldMap = null;

            return this;
        }

	    public IMapExpressionPostRoot<MODEL> FromFunction(Func<object> fieldValueMethod)
        {
            verifyFieldExpressionIsBeingConfigured();

            _currentFieldMap.FieldValueMethod = fieldValueMethod;

            MapVisitor.Visit(_currentFieldMap);

            _currentFieldMap = null;

            return this;
        }

	    public IMapExpressionPostBasedOnField<MODEL> BasedOnField(string fieldName)
        {
            verifyFieldExpressionIsBeingConfigured();

            _currentFieldMap.FieldNames = new[] { fieldName };

            return this;
        }

	    public IMapExpressionPostBasedOnField<MODEL> BasedOnIdentifyingField(string fieldName)
        {
            verifyFieldExpressionIsBeingConfigured();

	        _currentFieldMap.IsIdentifier = true;

            return BasedOnField(fieldName);
        }

	    public IMapExpressionPostBasedOnFields<MODEL> BasedOnFields(params string[] fieldNames)
        {
            verifyFieldExpressionIsBeingConfigured();

            _currentFieldMap.FieldNames = fieldNames;

            return this;
        }

	    public IMapExpressionPostRoot<MODEL> Do(Func<string[], object> mapFieldValuesToObject)
	    {
	        _currentFieldMap.MapFieldValuesToObject = mapFieldValuesToObject;

	        MapVisitor.Visit(_currentFieldMap);

	        _currentFieldMap = null;

	        return this;
	    }

	    public IMapExpressionPostRoot<MODEL> FromIdentifyingField(string fieldName)
        {
            _currentFieldMap.IsIdentifier = true;

            return FromField(fieldName);
        }

        public IMapExpressionPostRoot<MODEL> FromField(string fieldName)
        {
            return FromFields(new[] { fieldName });
        }

        public IMapExpressionPostRoot<MODEL> FromFields(params string[] fieldNames)
        {
            verifyFieldExpressionIsBeingConfigured();

            _currentFieldMap.FieldNames = fieldNames;
            MapVisitor.Visit(_currentFieldMap);

            _currentFieldMap = null;
            return this;
        }

        private void verifyFieldExpressionIsNotBeingConfigured()
        {
            if (_currentFieldMap != null)
            {
                throw new DovetailMappingException(1001, "Assign must be followed by a .To(string fieldName).");
            }
        }

        private void verifyFieldExpressionIsBeingConfigured()
        {
            if (_currentFieldMap == null)
            {
                throw new DovetailMappingException(1002, "To must be prepended by a .Assign( expression ).");
            }
        }

    	public IMapExpressionPostDoNotEncode<MODEL> DoNotEncode()
    	{
    		verifyFieldExpressionIsBeingConfigured();

    		_currentFieldMap.ShouldEncode = false;

    		return this;
    	}
    }
}
