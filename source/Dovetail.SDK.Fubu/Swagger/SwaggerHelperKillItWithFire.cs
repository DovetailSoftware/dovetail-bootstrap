namespace Dovetail.SDK.Fubu.Swagger
{
    public static class SwaggerHelperKillItWithFire
    {
        public static string GetAPIResourcePattern()
        {
            return "api/{GroupKey}.swagger";
        }

        public static string GetBasePathPattern()
        {
            //TODO get full server URL for basePath
            var basePath = "api.swagger";

            return basePath;
        }
    }
}