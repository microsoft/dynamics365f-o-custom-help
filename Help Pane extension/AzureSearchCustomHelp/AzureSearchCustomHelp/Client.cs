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
namespace AzureSearchCustomHelp
{
    public class Client
    {
        string searchServiceName = "";
        string queryApiKey = "";
        string indexName = "";
             
        public void setConfig(string _serviceName, string _queryKey, string _indexName)
        {
            searchServiceName = _serviceName;
            queryApiKey = _queryKey;
            indexName = _indexName;
        }
        public SearchIndexClient CreateSearchIndexClient()
        {
           
            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
            return indexClient;
        }
        
        public List<Document> searchResult(string _searchString, string _filter)
        {
            List <Document> doc = new List<Document>();
            
            SearchParameters parameters;
            Debugger.Launch();
            ISearchIndexClient indexClient = CreateSearchIndexClient();
            DocumentSearchResult<Document> results;

            parameters =
                 new SearchParameters()
                 {
                     Filter = _filter,
                     Select = new[] { "id", "ms_locale", "ms_search_region"
                                    ,"description","title","ms_search_form","metadata_storage_path","metadata_storage_content_type", "metadata_storage_name"}
                 };
            results = indexClient.Documents.Search<Document>(_searchString, parameters);

            foreach (SearchResult<Document> result in results.Results)
            {
                doc.Add(result.Document);
               
            }
            return doc;

        }

    }
}
