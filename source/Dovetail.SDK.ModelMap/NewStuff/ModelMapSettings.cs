using System;
using System.IO;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class ModelMapSettings
    {
        public ModelMapSettings()
        {
            Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
        }

        public string Directory { get; set; }
        public bool EnableCache { get; set; }
    }
}