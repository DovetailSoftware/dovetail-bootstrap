namespace Dovetail.SDK.ModelMap.Serialization
{
    public interface IObjectBuilder
    {
        ObjectBuilderResult Build(BuildObjectContext context);
    }
}