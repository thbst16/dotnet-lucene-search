using System.ComponentModel.DataAnnotations;

namespace search.Shared
{
    public class SearchModel{
        [Required]
        public string SearchText {get; set;}
        public int ResultsCount {get; set;}
        public List<WaffleText> SearchResults {get; set;}
    }
}