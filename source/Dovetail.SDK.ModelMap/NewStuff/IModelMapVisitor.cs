using Dovetail.SDK.ModelMap.NewStuff.Instructions;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IModelMapVisitor
    {
        void Visit(BeginModelMap instruction);
        void Visit(BeginTable instruction);
        void Visit(EndTable instruction);
        void Visit(BeginView instruction);
        void Visit(EndView instruction);
        void Visit(EndModelMap instruction);

        void Visit(BeginProperty instruction);
        void Visit(EndProperty instruction);
        void Visit(BeginAdHocRelation instruction);
        void Visit(BeginRelation instruction);
        void Visit(EndRelation instruction);
        void Visit(BeginMappedProperty instruction);
        void Visit(EndMappedProperty instruction);

        void Visit(BeginMappedCollection instruction);
        void Visit(EndMappedCollection instruction);
        void Visit(FieldSortMap instruction);
        void Visit(AddFilter instruction);
    }
}