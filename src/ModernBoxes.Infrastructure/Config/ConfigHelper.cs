using System;
using System.Configuration;

namespace ModernBoxes.Infrastructure
{
    public class ConfigHelper
    {
        private static Configuration configuration = null;

        public static Configuration MyConfiguration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    return configuration;
                }
                return configuration;
            }
        }

        public static void setConfig(String key, Object value)
        {
            if (MyConfiguration.AppSettings.Settings[key] != null)
            {
                MyConfiguration.AppSettings.Settings[key].Value = value.ToString();
            }
            else
            {
                MyConfiguration.AppSettings.Settings.Add(key, value.ToString());
            }
            MyConfiguration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static String getConfig(String key)
        {
            if (MyConfiguration.AppSettings.Settings[key] != null)
            {
                return MyConfiguration.AppSettings.Settings[key].Value;
            }
            else
            {
                return String.Empty;
            }
        }

        public const string ComponentLayoutKey = "componentLayout";
        private const string LegacyComponentLayoutKey = "compontentLayout";

        public static string GetComponentLayout()
        {
            var value = getConfig(ComponentLayoutKey);
            if (string.IsNullOrEmpty(value))
                value = getConfig(LegacyComponentLayoutKey);
            return value;
        }

        public static void SetComponentLayout(object value)
        {
            setConfig(ComponentLayoutKey, value);
            if (MyConfiguration.AppSettings.Settings[LegacyComponentLayoutKey] != null)
            {
                MyConfiguration.AppSettings.Settings.Remove(LegacyComponentLayoutKey);
                MyConfiguration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
    }
}