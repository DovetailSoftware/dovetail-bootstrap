using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class ModelMap
    {
        private readonly string _name;
        private readonly IList<IModelMapInstruction> _instructions = new List<IModelMapInstruction>();

        public ModelMap(string name)
        {
            _name = name;
        }

        public string Name {  get { return _name; } }

        public void AddInstruction(IModelMapInstruction instruction)
        {
            _instructions.Add(instruction);
        }

        public void Accept(IModelMapVisitor visitor)
        {
            _instructions.Each(_ => _.Accept(visitor));
        }
    }
}