using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dovetail.SDK.ModelMap.Legacy.Instructions;
using FubuCore.Reflection;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
    public class MapRelatedModelExpression<PARENTMODEL, CHILDMODEL> : IMapRelatedModelExpression<PARENTMODEL, CHILDMODEL>, IMapRelatedModelExpressionPostTo<PARENTMODEL, CHILDMODEL>
    {
        private readonly MapExpression<PARENTMODEL> _parentMapExpression;
        private readonly IModelMapVisitor _modelMapVisitor;

        public MapRelatedModelExpression(MapExpression<PARENTMODEL> parentMapExpression, IModelMapVisitor modelMapVisitor)
        {
            _parentMapExpression = parentMapExpression;
            _modelMapVisitor = modelMapVisitor;
        }

        public IMapRelatedModelExpressionPostTo<PARENTMODEL, CHILDMODEL> To(Expression<Func<PARENTMODEL, IEnumerable<CHILDMODEL>>> expression)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression);

            _modelMapVisitor.Visit(new BeginMapMany
                                      {
                                          ParentModelType = typeof(PARENTMODEL), 
                                          ChildModelType = typeof(CHILDMODEL), 
                                          MappedProperty = propertyInfo
                                      });

            return this;
        }

        public IMapRelatedModelExpressionPostTo<PARENTMODEL, CHILDMODEL> To(Expression<Func<PARENTMODEL, CHILDMODEL>> expression)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression);

            _modelMapVisitor.Visit(new BeginMapMany
                                      {
                                          ParentModelType = typeof(PARENTMODEL), 
                                          ChildModelType = typeof(CHILDMODEL), 
                                          MappedProperty = propertyInfo
                                      });

            return this;
        }

        public MapExpression<PARENTMODEL> ViaRelation(string relationName, Action<IMapExpressionPostRoot<CHILDMODEL>> mapAction)
        {
            var relationMapExpression = new MapExpression<CHILDMODEL>(_modelMapVisitor);

            _modelMapVisitor.Visit(new BeginRelation { RelationName = relationName });

            mapAction(relationMapExpression);

            _modelMapVisitor.Visit(new EndRelation());
            _modelMapVisitor.Visit(new EndMapMany());

            return _parentMapExpression;
        }

        public MapExpression<PARENTMODEL> ViaAdhocRelation(string fromFieldName, string toTableName, string toFieldName, Action<IMapExpressionPostRoot<CHILDMODEL>> mapAction)
        {
            var relationMapExpression = new MapExpression<CHILDMODEL>(_modelMapVisitor);

            _modelMapVisitor.Visit(new BeginAdHocRelation
                                      {
                                          ToTableFieldName = toFieldName, 
                                          FromTableField = fromFieldName, 
                                          ToTableName = toTableName
                                      });

            mapAction(relationMapExpression);

            _modelMapVisitor.Visit(new EndRelation());
            _modelMapVisitor.Visit(new EndMapMany());

            return _parentMapExpression;
        }
    }
}