using System;
using Dovetail.SDK.ModelMap.NewStuff;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff
{
    [TestFixture]
    public class PropertyTypesTester
    {
        [Test]
        public void parses_known_types()
        {
            PropertyTypes.Parse("int").ShouldEqual(typeof(int));
            PropertyTypes.Parse("string").ShouldEqual(typeof(string));
            PropertyTypes.Parse("dateTime").ShouldEqual(typeof(DateTime));
			PropertyTypes.Parse("decimal").ShouldEqual(typeof(decimal));
			PropertyTypes.Parse("bool").ShouldEqual(typeof(decimal));
			PropertyTypes.Parse("double").ShouldEqual(typeof(double));
			PropertyTypes.Parse("float").ShouldEqual(typeof(float));
			PropertyTypes.Parse("short").ShouldEqual(typeof(short));
		}
    }
}