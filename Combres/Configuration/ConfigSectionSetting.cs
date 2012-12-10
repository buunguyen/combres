#region License
// Copyright 2011 Buu Nguyen (http://www.buunguyen.net/blog)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://combres.codeplex.com
#endregion

using System.Configuration;

namespace Combres
{
    /// <summary>
    /// Represents the configuration section for the library.  
    /// </summary>
    /// <example>
    /// Register this config section in web.config with the following XML blocks:
    /// Inside <c>configSections</c> element:
    /// <code>
    /// <section name="combres" type="Combres.ConfigSectionSetting, Combres" />
    /// </code>
    /// Inside <c>configuration</c> element:
    /// <code>
    /// <combres definitionUrl="~/App_Data/data.xml" />
    /// </code>
    /// </example>
    public sealed class ConfigSectionSetting : ConfigurationSection
    {
        /// <summary>
        /// Creates an instance of <c>ConfigSectionSetting</c>.
        /// </summary>
        public static ConfigSectionSetting Get()
        {
            var sectionName = ConfigurationManager.AppSettings["CombresSectionName"] ?? "combres";
            return (ConfigSectionSetting)ConfigurationManager.GetSection(sectionName);
        }

        /// <summary>
        /// The location to the XML definition file.  Must be a relative ASP.NET path.
        /// </summary>
        [ConfigurationProperty("definitionUrl", IsRequired = true)]
        public string DefinitionUrl
        {
            get { return (string)this["definitionUrl"]; }
            set { this["definitionUrl"] = value; }
        }

        /// <summary>
        /// The fully-qualified class name of the log provider.
        /// </summary>
        [ConfigurationProperty("logProvider", IsRequired = false, DefaultValue = "Combres.Loggers.IgnoreLogger")]
        public string LogProvider
        {
            get { return (string)this["logProvider"]; }
            set { this["logProvider"] = value; }
        }

        /// <summary>
        /// The fully-qualified class name of the cache provider.
        /// </summary>
        [ConfigurationProperty("cacheProvider", IsRequired = false, DefaultValue = "Combres.Caches.AspNetResourceSetCache")]
        public string CacheProvider
        {
            get { return (string)this["cacheProvider"]; }
            set { this["cacheProvider"] = value; }
        }
    }
}
