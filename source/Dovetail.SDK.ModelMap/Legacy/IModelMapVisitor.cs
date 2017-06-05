using Dovetail.SDK.ModelMap.Legacy.Instructions;
using Dovetail.SDK.ModelMap.Legacy.ObjectModel;

namespace Dovetail.SDK.ModelMap.Legacy
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