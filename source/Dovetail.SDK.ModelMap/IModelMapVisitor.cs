using Dovetail.SDK.ModelMap.Instructions;

namespace Dovetail.SDK.ModelMap
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

	    void Visit(BeginTransform instruction);
	    void Visit(EndTransform instruction);
	    void Visit(AddTransformArgument instruction);
	    void Visit(RemoveProperty instruction);
	    void Visit(RemoveMappedProperty instruction);
	    void Visit(RemoveMappedCollection instruction);
	    void Visit(AddTag instruction);

	    void Visit(PushVariableContext instruction);
	    void Visit(PopVariableContext instruction);
    }
}