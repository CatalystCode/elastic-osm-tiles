
namespace Microsoft.PCT.Configuration
{
    using Azure;
    using System.Collections.Concurrent;

    /// <summary>
    /// Abstraction for retrieving configuration options.
    /// </summary>
    public static class ConfigurationHelper
    {
        private static readonly ConcurrentDictionary<string, string> s_cachedSettings =
            new ConcurrentDictionary<string, string>();

        /// <summary>
        /// -1 = unset, 0 = false, 1 = true
        /// </summary>
        private static byte s_shouldCache;

        private static bool ShouldCache
        {
            get
            {
                // Read setting exactly once
                if (s_shouldCache < 0)
                {
                    var setting = CloudConfigurationManager.GetSetting("CacheConfigurationSettings");

                    var result = default(bool);
                    if (setting == null || !bool.TryParse(setting, out result))
                    {
                        result = false;
                    }

                    s_shouldCache = (byte)(result ? 1 : 0);
                }

                return s_shouldCache > 0;
            }
        }

        /// <summary>
        /// Get a configuration setting with the given name.
        /// </summary>
        /// <param name="name">The setting name.</param>
        /// <returns>The setting value, or null.</returns>
        public static string GetSetting(string name)
        {
            return ShouldCache
                ? s_cachedSettings.GetOrAdd(name, GetSettingCore)
                : GetSettingCore(name);
        }

        private static string GetSettingCore(string name)
        {
            return CloudConfigurationManager.GetSetting(name);
        }
    }
}
