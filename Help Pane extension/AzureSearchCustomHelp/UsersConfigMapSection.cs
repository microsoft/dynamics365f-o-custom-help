using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchCustomHelp
{
    public sealed class UsersConfigMapSection : ConfigurationSection
    {
        static string languageConfigFile = "Language.config";
        private static UsersConfigMapSection config;

        public static UsersConfigMapSection Config
        {
            get
            {
                try
                {
                    ExeConfigurationFileMap customConfigFileMap = new ExeConfigurationFileMap();
                    customConfigFileMap.ExeConfigFilename = Path.Combine(Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath), languageConfigFile);
                    Configuration customConfig = ConfigurationManager.OpenMappedExeConfiguration(customConfigFileMap, ConfigurationUserLevel.None);
                    config = customConfig.GetSection("langauagesection") as UsersConfigMapSection;
                    return config;
                }
                catch (Exception )
                {
                    return null;
                }
            }
        }
        
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        private UsersConfigMapConfigElements Settings
        {
            get { return (UsersConfigMapConfigElements)this[""]; }
            set { this[""] = value; }
        }

        public IEnumerable<UsersConfigMapConfigElement> SettingsList
        {
            get { return this.Settings.Cast<UsersConfigMapConfigElement>(); }
        }
    }

    public sealed class UsersConfigMapConfigElements : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new UsersConfigMapConfigElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((UsersConfigMapConfigElement)element).PrimaryLanguage;
        }
    }

    public sealed class UsersConfigMapConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("language", IsKey = true, IsRequired = true)]
        public string PrimaryLanguage
        {
            get { return (string)base["language"]; }
            set { base["language"] = value; }
        }

        [ConfigurationProperty("parentlanguage", IsRequired = false)]
        public string ParentLanguage
        {
            get { return (string)base["parentlanguage"]; }
            set { base["parentlanguage"] = value; }
        }

        [ConfigurationProperty("index", IsRequired = false)]
        public string Index
        {
            get { return (string)base["index"]; }
            set { base["index"] = value; }
        }

        [ConfigurationProperty("parentindex", IsRequired = false)]
        public string ParentIndex
        {
            get { return (string)base["parentindex"]; }
            set { base["parentindex"] = value; }
        }

        [ConfigurationProperty("ulitmateindex", IsRequired = false)]
        public string UlitmateIndex
        {
            get { return (string)base["ulitmateindex"]; }
            set { base["ulitmateindex"] = value; }
        }
    }

    public class RootObject
    {
        [JsonProperty("@odata.context")]
        public string context { get; set; }
        public List<IndexName> value { get; set; }
    }
    public class IndexName
    {
        public string name { get; set; }
    }
}

