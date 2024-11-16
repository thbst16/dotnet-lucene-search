using Lucene.Net.Facet;
using System.ComponentModel.DataAnnotations;

namespace search.Shared
{
    public class SearchModel{
        [Required]
        public string SearchText {get; set;} = string.Empty;
        public int ResultsCount {get; set;}
        public int PageCount {get; set;}
        public int CurrentPage {get; set;}
        public List<WaffleText> CurrentPageSearchResults {get; set;}
        public IList<FacetResult> FacetResults { get; set; }
    }
}