namespace montaherul.Models
{
    public class SearchQueryModel
    {
        public List<FilterItem> filter { get; set; }
        public int page { get; set; }
        public int size { get; set; }
    }

    public class FilterItem
    {
        public string field { get; set; }
        public string type { get; set; }
        public string value { get; set; }
    }
}
