using System;
using System.Collections.Specialized;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Common.State;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests
{
    public class clarify_application_factory
    {
        [TestFixture]
        public class get_sdk_configuration : Context<ClarifyApplicationFactory>
        {
	        private DovetailDatabaseSettings _settings;

	        public override void OverrideMocks()
	        {
		        _settings = new DovetailDatabaseSettings { ConnectionString = "connstr", SessionTimeoutInMinutes = 1234, Type = "mssql" };
	        }

            [Test]
            public void should_contain_any_appsettings_starting_with_fchoice()
            {
				var configuration = ClarifyApplicationFactory.GetDovetailSDKConfiguration(_settings);

                configuration.Get("fchoice.test").ShouldNotBeNull();
            }

            [Test]
            public void should_set_state_manager_timeout_using_database_settings()
            {
                _cut.setSessionDefaultTimeout(_settings);

                StateManager.StateTimeout.ShouldEqual(TimeSpan.FromMinutes(_settings.SessionTimeoutInMinutes));
            }
        }

        [TestFixture]
        public class merge_sdk_settings
        {
            [Test]
            public void should_ignore_keys_not_starting_with_fchoice()
            {
                var source = new NameValueCollection { { "fchoice.key1", "source" } };
                var target = new NameValueCollection { { "dovetail.key2", "target" }, { "another.key2", "target" } };

                var result = ClarifyApplicationFactory.MergeSDKSettings(source, target);

                result.Count.ShouldEqual(1);
            }

            [Test]
            public void should_not_override_existing_settings()
            {
                var source = new NameValueCollection { { "fchoice.dbtype", "source-type" } };
                var target = new NameValueCollection { { "fchoice.dbtype", "target-type" } };

                var result = ClarifyApplicationFactory.MergeSDKSettings(source, target);

                result.Count.ShouldEqual(1);
                result["fchoice.dbtype"].ShouldEqual("source-type");
            }

            [Test]
            public void should_add_new_keys_to_result_settings()
            {
                var source = new NameValueCollection { { "fchoice.key1", "source" } };
                var target = new NameValueCollection { { "fchoice.key2", "target" } };

                var result = ClarifyApplicationFactory.MergeSDKSettings(source, target);

                result.Count.ShouldEqual(2);
                result["fchoice.key1"].ShouldEqual("source");
                result["fchoice.key2"].ShouldEqual("target");
            }

            [Test]
            public void should_not_modify_inputted_settings()
            {
                var source = new NameValueCollection { { "fchoice.key1", "source" } };
                var target = new NameValueCollection { { "fchoice.key2", "target" } };

                ClarifyApplicationFactory.MergeSDKSettings(source, target);

                source.Count.ShouldEqual(1);
                target.Count.ShouldEqual(1);
            }
        }
    }
}