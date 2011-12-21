using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.ObjectModel;

namespace Dovetail.SDK.ModelMap
{
    public interface IModelMapVisitor
    {
        void Visit(BeginModelMap beginModelMap);
        void Visit(BeginTable beginTable);
        void Visit(BeginView beginView);
        void Visit(BeginAdHocRelation beginAdHocRelation);
        void Visit(BeginRelation beginRelation);
        void Visit(EndRelation endRelation);
        void Visit(BeginMapMany beginMapMany);
        void Visit(EndMapMany endMapMany);
        void Visit(FieldMap fieldMap);
        void Visit(EndModelMap endModelMap);
        void Visit(FieldSortMap fieldSortMap);
		void Visit(AddFilter addFilter);
    }
}