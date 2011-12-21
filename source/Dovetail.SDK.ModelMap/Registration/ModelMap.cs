using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.Registration.DSL;

namespace Dovetail.SDK.ModelMap.Registration
{
    public abstract class ModelMap<MODEL>
    {
        private MapExpression<MODEL> _mapExpression;

        protected abstract void MapDefinition();

        public void Accept(IModelMapVisitor modelMapVisitor)
        {
            modelMapVisitor.Visit(new BeginModelMap { ModelType = typeof(MODEL) });

            _mapExpression = new MapExpression<MODEL>(modelMapVisitor);

            MapDefinition();

            modelMapVisitor.Visit(new EndModelMap());
        }

        public IMapExpressionPostRoot<MODEL> FromTable(string tableName)
        {
            return _mapExpression.FromTable(tableName);
        }

        public IMapExpressionPostView<MODEL> FromView(string viewName)
        {
            return _mapExpression.FromView(viewName);
        }
    }
}