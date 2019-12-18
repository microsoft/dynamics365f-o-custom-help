using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Diagnostics;
using System.Globalization;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Net;
using Newtonsoft.Json;

namespace AzureSearchCustomHelp
{
    public class Client
    {
        string searchServiceName = string.Empty;
        string searchServiceIndex = string.Empty;
        string queryApiKey = string.Empty;
        static string ultimateSearchServiceIndex = string.Empty;
        static string ultimateLanguage = string.Empty;
        static UsersConfigMapConfigElement languageConfigValues = null;
        static IndexName ultimateIndexNameFromAzure = new IndexName();
        static IndexName primaryIndexNameFromAzure = new IndexName();
        static IndexName parentIndexNameFromAzure = new IndexName();
        List<IndexName> indexList = new List<IndexName>();

        private bool ValidateUlimateSearchServiceIndex()
        {
            bool isIndexFound;
            if (!string.IsNullOrEmpty(ultimateSearchServiceIndex))
            {
                ultimateIndexNameFromAzure = indexList.Where(i => i.name == ultimateSearchServiceIndex).FirstOrDefault();
                isIndexFound = (ultimateIndexNameFromAzure != null) ? true : false;
            }
            else
            {
                isIndexFound = true;
            }
            return isIndexFound;
        }

        private List<IndexName> GetIndexListFromAzure()
        {
            List<IndexName> azureIndexList = null;
            string indexListResults = null;
            string Url = string.Format(@"https://{0}.search.windows.net/indexes?api-version=2017-11-11&$select=name", searchServiceName);

            WebRequest request = WebRequest.Create(Url);
            request.Headers.Add("api-key", queryApiKey);
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

        public SearchIndexClient CreateSearchIndexClient()
        {
            SearchIndexClient indexClient = null;
            return (!string.IsNullOrEmpty(searchServiceIndex)) ? new SearchIndexClient(searchServiceName, searchServiceIndex, new SearchCredentials(queryApiKey)) : indexClient;
        }

        private DocumentSearchResult<Document> GetUltimateLanguageSearchResult(string _searchString, string ultimateLanguage, string ulitmateindex)
        {
            searchServiceIndex = ulitmateindex;
            return SearchResult(_searchString, ultimateLanguage);
        }

        private static string GetDefaultIndex()
        {
            GetUltimateLanguageAndIndex();
            return ultimateSearchServiceIndex;
        }

        private static string GetDefaultLanguage()
        {
            GetUltimateLanguageAndIndex();
            return ultimateLanguage;
        }

        private static void GetUltimateLanguageAndIndex()
        {
            var isValidConfig = UsersConfigMapSection.Config;
            if (isValidConfig != null)
            {
                List<UsersConfigMapConfigElement> languages = UsersConfigMapSection.Config.SettingsList.ToList<UsersConfigMapConfigElement>();

                var ulimateValues = languages.Where(l => l.UlitmateIndex != string.Empty).FirstOrDefault();
                if (ulimateValues != null)
                {
                    ultimateSearchServiceIndex = ulimateValues.UlitmateIndex;
                    ultimateLanguage = ulimateValues.PrimaryLanguage;
                }
                else
                {
                    ultimateSearchServiceIndex = string.Empty;
                    ultimateLanguage = string.Empty;
                }
            }
        }

        private static UsersConfigMapConfigElement GetLanguageIndexes(string primaryLanguage)
        {
            var isValidConfig = UsersConfigMapSection.Config;
            if (isValidConfig != null)
            {
                List<UsersConfigMapConfigElement> languages = UsersConfigMapSection.Config.SettingsList.ToList<UsersConfigMapConfigElement>();
                UsersConfigMapConfigElement languageIndexes = languages.Where(l => l.PrimaryLanguage.ToLower() == primaryLanguage.ToLower()).FirstOrDefault();
                if (languageIndexes != null)
                {
                    if (!string.IsNullOrEmpty(languageIndexes.ParentLanguage))
                    {
                        UsersConfigMapConfigElement parentLanguage = languages.Where(l => l.PrimaryLanguage.ToLower() == languageIndexes.ParentLanguage.ToLower()).FirstOrDefault();
                        if (parentLanguage != null)
                        {
                            if (!string.IsNullOrEmpty(parentLanguage.ParentIndex))
                            {
                                languageIndexes.ParentLanguage = parentLanguage.PrimaryLanguage;
                                languageIndexes.ParentIndex = parentLanguage.ParentIndex;
                            }
                        }
                       
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(languageIndexes.ParentIndex))
                        {
                            if (string.IsNullOrEmpty(languageIndexes.Index))
                            {
                                languageIndexes.Index = languageIndexes.ParentIndex;
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(ultimateSearchServiceIndex))
                {
                    languageIndexes = new UsersConfigMapConfigElement();
                    languageIndexes.Index = ultimateSearchServiceIndex;
                }
                return languageIndexes;
            }
            return null;
        }

        private DocumentSearchResult<Document> SearchResult(string _searchString, string filterLanguage, string filterAnotherLanguage = "")
        {
            SearchParameters parameters;
            ISearchIndexClient indexClient = CreateSearchIndexClient();
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
                     Select = new[] { "id", "ms_locale", "ms_search_region"
                                    ,"description","title","ms_search_form","metadata_storage_path","metadata_storage_content_type", "metadata_storage_name"}
                 };
            try
            {
                if (indexClient != null)
                {
                    results = indexClient.Documents.Search<Document>(_searchString, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return results;
        }

        #region Calling Methods in XPP
        public static string GetParentLanguage(string primaryLanguage)
        {
            if (languageConfigValues != null && languageConfigValues.ParentLanguage != null && primaryLanguage != "ultimatelanguage")
            {
                return languageConfigValues.ParentLanguage;
            }
            else
            {
                UsersConfigMapConfigElement language = null;
                language = GetLanguageIndexes(primaryLanguage);
                return (language != null) ? language.ParentLanguage : string.Empty;
            }
        }
        public static bool CheckFileExists(string file)
        {
            var FilePath = Path.Combine(Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath), file);
            if (File.Exists(FilePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetUltimateLanguage()
        {
            var UltimateLanguage = GetDefaultLanguage();
            return UltimateLanguage;
        }

        public Boolean SetConfig(string _serviceName, string _queryKey, string _primaryLanguage)
        {
            Boolean isIndexFound = false;
            searchServiceName = _serviceName;
            queryApiKey = _queryKey;
            indexList = GetIndexListFromAzure();
            if (indexList.Count > 0)
            {

                ultimateSearchServiceIndex = GetDefaultIndex();
                ultimateLanguage = GetDefaultLanguage();

                languageConfigValues = GetLanguageIndexes(_primaryLanguage);
                if (languageConfigValues == null)
                {
                    isIndexFound = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(languageConfigValues.Index))
                    {
                        primaryIndexNameFromAzure = indexList.Where(i => i.name == languageConfigValues.Index).FirstOrDefault();
                        if (primaryIndexNameFromAzure != null)
                        {
                            if (!string.IsNullOrEmpty(languageConfigValues.ParentIndex))
                            {
                                parentIndexNameFromAzure = indexList.Where(i => i.name == languageConfigValues.ParentIndex).FirstOrDefault();
                                if (parentIndexNameFromAzure != null)
                                {
                                    isIndexFound = ValidateUlimateSearchServiceIndex();
                                }
                                else
                                {
                                    isIndexFound = false;
                                }
                            }
                            else
                            {
                                isIndexFound = ValidateUlimateSearchServiceIndex();
                            }
                        }
                        else
                        {
                            isIndexFound = false;
                        }
                    }
                    else if (!string.IsNullOrEmpty(languageConfigValues.ParentIndex))
                    {
                        parentIndexNameFromAzure = indexList.Where(i => i.name == languageConfigValues.ParentIndex).FirstOrDefault();
                        if (parentIndexNameFromAzure != null)
                        {
                            isIndexFound = ValidateUlimateSearchServiceIndex();
                        }
                        else
                        {
                            isIndexFound = false;
                        }
                    }
                    else
                    {
                        isIndexFound = ValidateUlimateSearchServiceIndex();
                    }
                }
            }
            return isIndexFound;
        }

        public List<Document> searchResult(string _searchString, string _filter, string _primaryLanguage, bool _tIsUserSearch = true)
        {
            List<Document> documents = new List<Document>();

            //languageConfigValues = GetLanguage(_primaryLanguage);
            DocumentSearchResult<Document> resultDocuments = null;
            if (languageConfigValues != null)
            {
                searchServiceIndex = languageConfigValues.Index;

                //Defualt Search
                if (_tIsUserSearch == false)
                {
                    if (languageConfigValues.PrimaryLanguage.ToLower() == ultimateLanguage.ToLower())
                    {
                        if (!string.IsNullOrEmpty(ultimateSearchServiceIndex))
                        {
                            searchServiceIndex = ultimateSearchServiceIndex;
                        }
                    }
                    resultDocuments = SearchResult(_searchString, languageConfigValues.PrimaryLanguage);
                }

                //UserSearch
                else
                {
                    if (!string.IsNullOrEmpty(languageConfigValues.ParentLanguage))
                    {
                        //Check PrimaryIndex and ParentIndex are same

                        if (languageConfigValues.Index == languageConfigValues.ParentIndex)
                        {
                            if (!string.IsNullOrEmpty(languageConfigValues.Index) && !string.IsNullOrEmpty(languageConfigValues.ParentIndex))
                            {
                                //Search in Client and Parent language only.
                                resultDocuments = SearchResult(_searchString, languageConfigValues.PrimaryLanguage, languageConfigValues.ParentLanguage);
                            }

                            if (resultDocuments == null)
                            {
                                if (!string.IsNullOrEmpty(ultimateLanguage))
                                {
                                    if (languageConfigValues.ParentLanguage.ToLower() != ultimateLanguage.ToLower())
                                    {
                                        //Search in UltimateLanguage with ulitmateindex
                                        var ultimateLanguageSearchResult = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, ultimateSearchServiceIndex.ToLower());
                                        if (ultimateLanguageSearchResult != null)
                                        {
                                            resultDocuments = ultimateLanguageSearchResult;
                                        }
                                    }
                                    else if (languageConfigValues.ParentLanguage.ToLower() == ultimateLanguage.ToLower() && string.IsNullOrEmpty(languageConfigValues.ParentIndex))
                                    {
                                        //Search in UltimateLanguage with ulitmateindex
                                        var ultimateLanguageSearchResult = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, ultimateSearchServiceIndex.ToLower());
                                        if (ultimateLanguageSearchResult != null)
                                        {
                                            resultDocuments = ultimateLanguageSearchResult;
                                        }
                                    }
                                }
                            }

                            //Filter Primary Search Result
                            else if (resultDocuments.Results.Count > 0)
                            {
                                //Results for Primary language
                                var doc = resultDocuments.Results.Where(x => x.Document.ms_locale == languageConfigValues.PrimaryLanguage.ToLower()).ToList<SearchResult<Document>>();
                                if (doc.Count > 0)
                                {
                                    resultDocuments.Results = doc;
                                }
                                else //Results with Parent Language
                                {
                                    if (!string.IsNullOrEmpty(ultimateLanguage))
                                    {
                                        if (languageConfigValues.ParentLanguage.ToLower() != ultimateLanguage.ToLower())
                                        {
                                            //Search in UltimateLanguage with ulitmateindex
                                            var ultimateLanguageSearchResult = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, ultimateSearchServiceIndex.ToLower());
                                            if (ultimateLanguageSearchResult != null)
                                            {
                                                foreach (var item in ultimateLanguageSearchResult.Results)
                                                {
                                                    resultDocuments.Results.Add(item); // Add Ulitmate Language search to Parent Language search results.
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Search in UltimateLanguage with ulitmateindex
                                if (resultDocuments.Results.Count == 0)
                                {
                                    resultDocuments = SearchResult(_searchString, ultimateLanguage);
                                }
                            }
                        }
                        else //PrimaryIndex and ParentIndex are different
                        {
                            if (!string.IsNullOrEmpty(languageConfigValues.Index))  //Search in PrimaryLanguage with primaryIndex
                            {
                                searchServiceIndex = languageConfigValues.Index;
                                resultDocuments = SearchResult(_searchString, languageConfigValues.PrimaryLanguage);
                            }

                            if (resultDocuments == null)
                            {
                                //Check parentindex and ulitmateindex are same
                                if (languageConfigValues.ParentIndex.ToLower() == ultimateSearchServiceIndex.ToLower())
                                {
                                    //Search in Client and Parent language only.
                                    resultDocuments = SearchResult(_searchString, languageConfigValues.ParentLanguage, ultimateLanguage);
                                }
                                else //parentindex and ulitmateindex are different
                                {
                                    if (!string.IsNullOrEmpty(languageConfigValues.ParentIndex)) //Search in ParentLanguage with ParentIndex
                                    {
                                        searchServiceIndex = languageConfigValues.ParentIndex;
                                        resultDocuments = SearchResult(_searchString, languageConfigValues.ParentLanguage);
                                    }

                                    if (!string.IsNullOrEmpty(ultimateLanguage))
                                    {
                                        if (languageConfigValues.ParentLanguage.ToLower() != ultimateLanguage.ToLower())
                                        {
                                            //Search in UltimateLanguage with ulitmateindex
                                            var ultimateLanguageSearchResult = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, ultimateSearchServiceIndex.ToLower());
                                            if (ultimateLanguageSearchResult != null)
                                            {
                                                resultDocuments = ultimateLanguageSearchResult;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (resultDocuments.Results.Count == 0)
                            {
                                //Check parentindex and ulitmateindex are same
                                if (languageConfigValues.ParentIndex.ToLower() == ultimateSearchServiceIndex.ToLower())
                                {
                                    //Search in Client and Parent language only.
                                    resultDocuments = SearchResult(_searchString, languageConfigValues.ParentLanguage, ultimateLanguage);
                                }
                                else //parentindex and ulitmateindex are different
                                {
                                    if (!string.IsNullOrEmpty(languageConfigValues.ParentIndex)) //Search in ParentLanguage with ParentIndex
                                    {
                                        searchServiceIndex = languageConfigValues.ParentIndex;
                                        resultDocuments = SearchResult(_searchString, languageConfigValues.ParentLanguage);
                                    }

                                    if (!string.IsNullOrEmpty(ultimateLanguage))
                                    {
                                        if (languageConfigValues.ParentLanguage.ToLower() != ultimateLanguage.ToLower())
                                        {
                                            //Search in UltimateLanguage with ulitmateindex
                                            var ultimateLanguageSearchResult = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, ultimateSearchServiceIndex.ToLower());
                                            if (ultimateLanguageSearchResult != null)
                                            {
                                                if (resultDocuments == null)
                                                {
                                                    resultDocuments = new DocumentSearchResult<Document>();
                                                }
                                                foreach (var item in ultimateLanguageSearchResult.Results)
                                                {
                                                    resultDocuments.Results.Add(item); // Add Ulitmate Language search to Parent Language search results.
                                                }
                                            }
                                        }
                                        else if(languageConfigValues.ParentIndex.ToLower()!= ultimateSearchServiceIndex.ToLower())
                                        {
                                            //Search in UltimateLanguage with ulitmateindex
                                            var ultimateLanguageSearchResult = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, ultimateSearchServiceIndex.ToLower());
                                            if (ultimateLanguageSearchResult != null)
                                            {
                                                if (resultDocuments == null)
                                                {
                                                    resultDocuments = new DocumentSearchResult<Document>();
                                                }
                                                foreach (var item in ultimateLanguageSearchResult.Results)
                                                {
                                                    resultDocuments.Results.Add(item); // Add Ulitmate Language search to Parent Language search results.
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Search in Client Language only.
                        resultDocuments = SearchResult(_searchString, languageConfigValues.PrimaryLanguage);
                        if (resultDocuments == null && !string.IsNullOrEmpty(ultimateLanguage) && _searchString != null && _tIsUserSearch == true)
                        {
                            //Search in UltimateLanguage with ulitmateindex
                            resultDocuments = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, ultimateSearchServiceIndex.ToLower());
                        }
                        else if (resultDocuments != null && !string.IsNullOrEmpty(ultimateLanguage) && resultDocuments.Results.Count == 0 && _searchString != null && _tIsUserSearch == true)
                        {
                            //Search in UltimateLanguage with ulitmateindex
                            resultDocuments = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, ultimateSearchServiceIndex.ToLower());
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_primaryLanguage))
                {
                    resultDocuments = SearchResult(_searchString, _primaryLanguage);
                }

                if (!string.IsNullOrEmpty(ultimateLanguage))
                {
                    //Search in UltimateLanguage with ulitmateindex
                    if (resultDocuments.Results.Count == 0 && _searchString != null && _tIsUserSearch == true && ultimateLanguage != null)
                    {
                        resultDocuments = GetUltimateLanguageSearchResult(_searchString, ultimateLanguage, GetDefaultIndex());
                    }
                }
            }

            if (resultDocuments != null)
            {
                if (resultDocuments.Results.Count > 0)
                {
                    documents = resultDocuments.Results.OrderByDescending(search => search.Score).Select(resultlist => resultlist.Document).ToList();

                    foreach (var doc in documents)
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
        #endregion

    }
}
