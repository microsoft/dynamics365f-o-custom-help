using System;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using System.ComponentModel;
using Newtonsoft.Json;
namespace AzureSearchCustomHelp
{
    
       

    // The SerializePropertyNamesAsCamelCase attribute is defined in the Azure Search .NET SDK.
    // It ensures that Pascal-case property names in the model class are mapped to camel-case
    // field names in the index.
    [SerializePropertyNamesAsCamelCase]
    public partial class Document
{
        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable, IsSortable, IsFacetable, IsSearchable]
        public string id { get; set; }

        [IsFilterable, IsSortable, IsFacetable, IsSearchable]
        public string Title { get; set; }

        [IsFilterable, IsSortable, IsFacetable, IsSearchable]
        public string ms_search_form { get; set; }

        [IsFilterable, IsSortable, IsFacetable, IsSearchable]
        public string metadata_storage_name { get; set; }

        [IsFilterable, IsSortable, IsFacetable, IsSearchable]
        public string ms_search_region { get; set; }

        [IsFilterable, IsSortable, IsFacetable, IsSearchable]
        public string ms_locale { get; set; }

        [IsFilterable, IsSortable, IsFacetable, IsSearchable]
        public string metadata_storage_path { get; set; }

       [System.ComponentModel.DataAnnotations.Key]
       [IsSearchable]
       public string Microsoft_Help_Id { get; set; }

       [IsFilterable, IsSortable, IsFacetable]
       public string metadata_storage_content_type { get; set; }


       [IsFilterable, IsSortable, IsFacetable, IsSearchable]
       public string description { get; set; }

       [IsFilterable, IsSortable, IsFacetable, IsSearchable]
       public string Content { get; set; }

      
    }

}
