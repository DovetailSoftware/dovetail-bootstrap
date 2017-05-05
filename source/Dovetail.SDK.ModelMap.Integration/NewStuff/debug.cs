using System.Diagnostics;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.Clarify;
using Dovetail.SDK.ModelMap.NewStuff;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FChoice.Foundation.Schema;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff
{
    [TestFixture]
    public class debug : MapFixture
    {
        [Test]
        public void hello()
        {
            var map = new ModelMap.NewStuff.ModelMap("case");
            map.AddInstruction(new BeginModelMap("case"));
            map.AddInstruction(new BeginTable("case"));

            map.AddInstruction(new BeginProperty
            {
                Key = "id",
                Field = "objid",
                DataType = "int",
                PropertyType = "identifier"
            });
            map.AddInstruction(new EndProperty());

            map.AddInstruction(new BeginProperty
            {
                Key = "caseId",
                Field = "id_number",
                DataType = "string"
            });
            map.AddInstruction(new EndProperty());

            map.AddInstruction(new BeginProperty
            {
                Key = "title",
                Field = "title",
                DataType = "string"
            });
            map.AddInstruction(new EndProperty());

            map.AddInstruction(new EndTable());
            map.AddInstruction(new EndModelMap());

            var builder = Container.GetInstance<ModelMap.NewStuff.MapEntryBuilder>();
            var models = new ModelBuilder(new StubRegistry(map), Container.GetInstance<ISchemaCache>(),
                Container.GetInstance<IOutputEncoder>(), builder, Container.GetInstance<IClarifyListCache>(), new InMemoryServiceLocator());

            var @case = models.GetOne("case", 268435890);
            Debug.WriteLine(@case.ToValues().ToString());
        }

        private class StubRegistry : IModelMapRegistry
        {
            private readonly ModelMap.NewStuff.ModelMap _map;

            public StubRegistry(ModelMap.NewStuff.ModelMap map)
            {
                _map = map;
            }

            public ModelMap.NewStuff.ModelMap Find(string name)
            {
                return _map;
            }

	        public ModelMap.NewStuff.ModelMap FindPartial(string name)
	        {
		        throw new System.NotImplementedException();
	        }
        }
    }
}