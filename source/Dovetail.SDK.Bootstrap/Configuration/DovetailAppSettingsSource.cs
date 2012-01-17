using System.Collections.Generic;
using System.Configuration;
using FubuCore.Configuration;

namespace Dovetail.SDK.Bootstrap.Configuration
{
    public class DovetailAppSettingsSource : ISettingsSource
    {
        public IEnumerable<SettingsData> FindSettingData()
        {
            var appSettings = ConfigurationManager.AppSettings;

            var data = new SettingsData(SettingCategory.profile) { Provenance = "applicationConfiguration/appSettings"};

            appSettings.AllKeys.Each(key =>
                                         {
                                             var value = appSettings[key];
                                             var keyWithSettings = appendSettingsToKeyTypeName(key);
                                             data[keyWithSettings] = value;
                                         });

            return new[] { data };
        }

        private static string appendSettingsToKeyTypeName(string key)
        {
            //Looking for settings that look like :
            // <ClassName.<Property>
            // or 
            // <ClassName>Settings.<Property>

            //don't mess with fchoice keys
            if (key.StartsWith("fchoice")) return key; 
            
            //no dots means we don't care
            var dotIndex = key.IndexOf('.');
            if (dotIndex == -1) return key;

            if (key.EndsWith("Settings")) return key;

            return key.Substring(0, dotIndex) + "Settings" + key.Substring(dotIndex);
        }
    }
}