using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Authentication;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests
{
    [TestFixture]
    public class SecurityModuleTester
    {
        [Test]
        [TestCase("", "")]
        [TestCase("jpg", ".jpg")]
        [TestCase(".jpg", ".jpg")]
        [TestCase("jpg css", ".jpg|.css")]
        [TestCase("jpg, css; .htm", ".jpg|.css|.htm")]
        [TestCase(" jpg  CSS;     html", ".jpg|.CSS|.html")]
        public void parse_the_whitelist_extension_setting(string setting, string parsedValues)
        {
            SecurityModule.GetWhiteListedExtensions(setting).Join("|").ShouldEqual(parsedValues);
        }

        [Test]
        [TestCase("", "file", true)]
        [TestCase("jpg", "", true)]
        [TestCase("jpg", "jpg", true)]
        [TestCase("jpg", "image", true)]
        [TestCase("jpg", "image.jpg", false)]
        [TestCase("css jpg", "image.JPG", false)]
        [TestCase("css jpg", "/app/folder/content/image.jpg", false)]
        [TestCase("css jpg", @"app\image.jpg", false)]
        public void requests_that_require_the_principal_to_load(string extensions, string path, bool requiresPrincipal)
        {
            var securityModule = new SecurityModule();
            securityModule.InitializeWhiteList(extensions);
            securityModule.PathRequiresPrincipal(path).ShouldEqual(requiresPrincipal);
        }
    }
}