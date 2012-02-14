using StructureMap.Configuration.DSL;

namespace FubuMVC.Swagger
{
    public class SwaggerRegistry: Registry
    {
        public SwaggerRegistry()
        {
            Scan(scan =>
                     {
                         scan.TheCallingAssembly();
                         scan.WithDefaultConventions();
                     });
        }
    }
}