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
        }
    }
}