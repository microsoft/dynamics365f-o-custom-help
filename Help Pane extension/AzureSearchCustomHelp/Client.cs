using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.IO;
using System.Reflection;
using System.Net;
using Newtonsoft.Json;

namespace AzureSearchCustomHelp
{
    public class Client
    {
        private string searchServiceName = string.Empty;
        private string queryApiKey = string.Empty;
        private SearchIndexClient primaryIndexClient = null;
        private SearchIndexClient parentIndexClient = null;
        private SearchIndexClient ultimateIndexClient = null;
        private string parentLanguage = string.Empty;
        private string ultimateLanguage = string.Empty;

        public Boolean SetConfig(string _serviceName, string _queryKey, string _primaryLanguage)
        {
            this.searchServiceName = _serviceName;
            this.queryApiKey = _queryKey;
            List<IndexName> indexList = GetIndexListFromAzure();
            if (indexList.Count > 0)
            {
                UsersConfigMapSection config = UsersConfigMapSection.Config;
                if (config != null)
                {
                    List<UsersConfigMapConfigElement> languages = config.SettingsList.ToList<UsersConfigMapConfigElement>();
                    if (languages != null)
                    {
                        try
                        {
                            SetUltimateIndex(indexList, languages);
                            SetParentIndex(indexList, languages, _primaryLanguage);
                            SetPrimaryIndex(indexList, languages, _primaryLanguage);
                        }
                        catch (InvalidConfigurationException)
                        {
                            return false;
                        }
                    }
                }
            }
            return primaryIndexClient != null || parentIndexClient != null || ultimateIndexClient != null;
        }

        private void SetUltimateIndex(List<IndexName> indexList, List<UsersConfigMapConfigElement> languages)
        {
            UsersConfigMapConfigElement ultimateLanguageConfig = languages.Where(l => l.UltimateIndex != string.Empty).FirstOrDefault();
            if (ultimateLanguageConfig != null && (null != indexList.Where(i => i.name == ultimateLanguageConfig.UltimateIndex).FirstOrDefault()))
            {
                this.ultimateIndexClient = CreateSearchIndexClient(ultimateLanguageConfig.UltimateIndex);
                this.ultimateLanguage = ultimateLanguageConfig.PrimaryLanguage;
            }
        }

        private void SetPrimaryIndex(List<IndexName> indexList, List<UsersConfigMapConfigElement> languages, string primaryLanguage)
        {
            UsersConfigMapConfigElement primaryLanguageConfig = languages.Where(l => l.PrimaryLanguage.ToLower() == primaryLanguage.ToLower()).FirstOrDefault();
            if (primaryLanguageConfig != null)
            {
                if (!string.IsNullOrEmpty(primaryLanguageConfig.Index) && (null != indexList.Where(i => i.name == primaryLanguageConfig.Index).FirstOrDefault()))
                {
                    //Set primaryIndex to the value of @index
                    this.primaryIndexClient = CreateSearchIndexClient(primaryLanguageConfig.Index);
                }
            }
        }

        private void SetParentIndex(List<IndexName> indexList, List<UsersConfigMapConfigElement> languages, string primaryLanguage)
        {
            UsersConfigMapConfigElement primaryLanguageConfig = languages.Where(l => l.PrimaryLanguage.ToLower() == primaryLanguage.ToLower()).FirstOrDefault();
            if (primaryLanguageConfig != null)
            {
                if (!string.IsNullOrEmpty(primaryLanguageConfig.ParentIndex) && !string.IsNullOrEmpty(primaryLanguageConfig.ParentLanguage))
                {
                    throw new InvalidConfigurationException("Both ParentIndex and ParentLanguage cannot be set for " + primaryLanguage);
                }
                if (!string.IsNullOrEmpty(primaryLanguageConfig.ParentIndex) && (null != indexList.Where(i => i.name == primaryLanguageConfig.ParentIndex).FirstOrDefault()))
                {
                    //Set parentIndex to the value of @parentindex.
                    this.parentIndexClient = CreateSearchIndexClient(primaryLanguageConfig.ParentIndex);
                    this.parentLanguage = primaryLanguageConfig.PrimaryLanguage;
                }
                if (!string.IsNullOrEmpty(primaryLanguageConfig.ParentLanguage))
                {
                    //Set the parentIndex to the value of @parentindex from the first ancestor that has @parentindex set.
                    string ancestorLanguageWithParentIndex = GetAncestorWithParentIndexLanguage(languages, primaryLanguageConfig.ParentLanguage);
                    if (!string.IsNullOrEmpty(ancestorLanguageWithParentIndex))
                    {
                        UsersConfigMapConfigElement ancestorLanguageConfig = languages.Where(l => l.PrimaryLanguage.ToLower() == ancestorLanguageWithParentIndex.ToLower()).FirstOrDefault();
                        if (ancestorLanguageConfig != null && !string.IsNullOrEmpty(ancestorLanguageConfig.ParentIndex))
                        {
                            this.parentIndexClient = CreateSearchIndexClient(ancestorLanguageConfig.ParentIndex);
                            this.parentLanguage = ancestorLanguageConfig.PrimaryLanguage;
                        }
                    }
                }
            }
        }

        private static string GetAncestorWithParentIndexLanguage(List<UsersConfigMapConfigElement> languages, string parentLanguage)
        {
            UsersConfigMapConfigElement currentParentLanguageConfig = languages.Where(l => l.PrimaryLanguage.ToLower() == parentLanguage.ToLower()).FirstOrDefault();
            if (currentParentLanguageConfig != null)
            {
                if (!string.IsNullOrEmpty(currentParentLanguageConfig.ParentLanguage) && !string.IsNullOrEmpty(currentParentLanguageConfig.ParentIndex))
                {
                    //Invalid config. @parentlanguage and @parentindex cannot both be set for @language.
                    throw new InvalidConfigurationException("Both ParentIndex and ParentLanguage cannot be set for " + parentLanguage);
                }
                if (!string.IsNullOrEmpty(currentParentLanguageConfig.ParentLanguage))
                {
                    //Look at the next ancestor.
                    return GetAncestorWithParentIndexLanguage(languages, currentParentLanguageConfig.ParentLanguage);
                }
                if (!string.IsNullOrEmpty(currentParentLanguageConfig.ParentIndex))
                {
                    //We've found an ancestor that has a ParentIndex set so return the language.
                    return currentParentLanguageConfig.PrimaryLanguage;
                }
                //Invalid config. @parentlanguage has been set, but an ancestor with a @parentindex cannot be found.
                throw new InvalidConfigurationException("parentlanguage has been set, but an ancestor with a parentindex cannot be found");
            }
            throw new InvalidConfigurationException("Invalid configuration for " + parentLanguage);
        }

        private List<IndexName> GetIndexListFromAzure()
        {
            List<IndexName> azureIndexList = null;
            string indexListResults = null;
            string Url = string.Format(@"https://{0}.search.windows.net/indexes?api-version=2017-11-11&$select=name", this.searchServiceName);

            WebRequest request = WebRequest.Create(Url);
            request.Headers.Add("api-key", this.queryApiKey);
            request.ContentType = "application/json";
            WebResponse response = request.GetResponse();

            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                indexListResults = sr.ReadToEnd();
            }

            //Convert Json Response Result to List.
            if (!string.IsNullOrEmpty(indexListResults))
            {
                azureIndexList = JsonConvert.DeserializeObject<RootObject>(indexListResults).value;
            }
            return azureIndexList;
        }

        #region Methods available in XPP
        public static string GetParentLanguage(string primaryLanguage)
        {
            UsersConfigMapSection config = UsersConfigMapSection.Config;
            if (config != null)
            {
                List<UsersConfigMapConfigElement> languages = config.SettingsList.ToList<UsersConfigMapConfigElement>();
                UsersConfigMapConfigElement primaryLanguageConfig = languages.Where(l => l.PrimaryLanguage.ToLower() == primaryLanguage.ToLower()).FirstOrDefault();
                if (primaryLanguageConfig != null)
                {
                    if (!string.IsNullOrEmpty(primaryLanguageConfig.ParentLanguage) && !string.IsNullOrEmpty(primaryLanguageConfig.ParentIndex))
                    {
                        //Invalid config. @parentlanguage and @parentindex cannot both be set for @language.
                        return string.Empty;
                    }
                    if (!string.IsNullOrEmpty(primaryLanguageConfig.ParentLanguage))
                    {
                        try
                        {
                            return GetAncestorWithParentIndexLanguage(languages, primaryLanguageConfig.ParentLanguage);
                        }
                        catch (InvalidConfigurationException)
                        {
                            return string.Empty;
                        }
                    }
                }
            }
            return string.Empty;
        }

        public static string GetUltimateLanguage()
        {
            UsersConfigMapSection config = UsersConfigMapSection.Config;
            if (config != null)
            {
                List<UsersConfigMapConfigElement> languages = config.SettingsList.ToList<UsersConfigMapConfigElement>();
                UsersConfigMapConfigElement ultimateLanguageConfig = languages.Where(l => l.UltimateIndex != string.Empty).FirstOrDefault();
                if (ultimateLanguageConfig != null)
                {
                    return ultimateLanguageConfig.PrimaryLanguage;
                }
            }
            return string.Empty;
        }


        public static bool CheckFileExists(string file)
        {
            string FilePath = Path.Combine(Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath), file);
            return (File.Exists(FilePath));
        }

        #endregion

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private SearchIndexClient CreateSearchIndexClient(string searchServiceIndex)
        {
            return (!string.IsNullOrEmpty(searchServiceIndex)) ? new SearchIndexClient(this.searchServiceName, searchServiceIndex, new SearchCredentials(this.queryApiKey)) : null;
        }

        private DocumentSearchResult<Document> ProcessSearch(SearchIndexClient searchIndexClient, string _searchString, string filterLanguage, string filterAnotherLanguage = "")
        {
            SearchParameters parameters;
            DocumentSearchResult<Document> results = null;
            string _filter = string.Empty;
            if (string.IsNullOrEmpty(filterAnotherLanguage))
            {
                _filter = string.Format("ms_locale eq '{0}' or ms_locale eq '{1}' or ms_locale eq '{2}'", filterLanguage.ToLower(), filterLanguage.ToUpper(), filterLanguage);
            }
            else
            {
                _filter = string.Format("ms_locale eq '{0}' or ms_locale eq '{1}' or ms_locale eq '{2}' or ms_locale eq '{3}' or ms_locale eq '{4}' or ms_locale eq '{5}'", filterLanguage.ToLower(), filterLanguage.ToUpper(), filterAnotherLanguage.ToLower(), filterAnotherLanguage.ToUpper(), filterLanguage, filterAnotherLanguage);
            }
            parameters =
                 new SearchParameters()
                 {
                     Filter = _filter,
                     Select = new[] { "id", "ms_locale", "ms_search_region", "description", "title", "ms_search_form", "metadata_storage_path", "metadata_storage_content_type", "metadata_storage_name"},
                     IncludeTotalResultCount = true
                 };
            try
            {
                if (searchIndexClient != null)
                {
                    results = searchIndexClient.Documents.Search<Document>(_searchString, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return results;
        }

        public List<Document> searchResult(string _searchString, string _filter, string _primaryLanguage, bool _tIsUserSearch = true)
        {
            List<Document> amalgamatedDocuments = new List<Document>();
            if (primaryIndexClient != null)
            {
                if (parentIndexClient != null && String.Equals(primaryIndexClient.IndexName, parentIndexClient.IndexName, StringComparison.OrdinalIgnoreCase))
                {
                    amalgamatedDocuments.AddRange(OrderAndProcessSearchResults(ProcessSearch(primaryIndexClient, _searchString, _primaryLanguage, parentLanguage)));
                }
                else
                {
                    amalgamatedDocuments.AddRange(OrderAndProcessSearchResults(ProcessSearch(primaryIndexClient, _searchString, _primaryLanguage)));
                }
            }
            else if (parentIndexClient != null && String.Equals(_primaryLanguage, parentLanguage, StringComparison.OrdinalIgnoreCase))
            {
                //Search _primaryLanguage when it is a parent language
                amalgamatedDocuments.AddRange(OrderAndProcessSearchResults(ProcessSearch(parentIndexClient, _searchString, _primaryLanguage)));
            }
            else if (ultimateIndexClient != null && String.Equals(_primaryLanguage, ultimateLanguage, StringComparison.OrdinalIgnoreCase))
            {
                //Search _primaryLanguage when it is the ultimate language
                amalgamatedDocuments.AddRange(OrderAndProcessSearchResults(ProcessSearch(ultimateIndexClient, _searchString, _primaryLanguage)));
            }
            if (amalgamatedDocuments.Count == 0)
            {
                if (parentIndexClient != null)
                {
                    amalgamatedDocuments.AddRange(OrderAndProcessSearchResults(ProcessSearch(parentIndexClient, _searchString, parentLanguage)));
                }
                if (ultimateIndexClient != null)
                {
                    amalgamatedDocuments.AddRange(OrderAndProcessSearchResults(ProcessSearch(ultimateIndexClient, _searchString, ultimateLanguage)));
                }
            }
            return amalgamatedDocuments;
        }

        private List<Document> OrderAndProcessSearchResults(DocumentSearchResult<Document> resultDocuments)
        {
            List<Document> documents = new List<Document>();
            if (resultDocuments != null)
            {
                if (resultDocuments.Results.Count > 0)
                {
                    documents = resultDocuments.Results.OrderByDescending(search => search.Score).Select(resultlist => resultlist.Document).ToList();

                    foreach (Document doc in documents)
                    {
                        if (!string.IsNullOrEmpty(doc.metadata_storage_path) && doc.metadata_storage_path.Contains(".json") && !string.IsNullOrEmpty(doc.ms_locale))
                        {
                            doc.metadata_storage_path = doc.metadata_storage_path.ToLower();
                            doc.metadata_storage_path = (doc.metadata_storage_path.Remove(0, doc.metadata_storage_path.IndexOf(doc.ms_locale.ToLower() + "/"))).Replace(".json", ".html");
                        }
                    }
                }
            }
            return documents;
        }
    }
}
