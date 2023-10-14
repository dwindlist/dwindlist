namespace dwindlist.Models.ViewModel
{
    public class FilteredList
    {
        public List<FilteredItem> Items { get; set; } = new List<FilteredItem>();
        public string Label { get; set; } = "Incomplete";
    }

    public class FilteredItem
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public char Status { get; set; } = 'i';
        public List<Breadcrumb> Breadcrumbs { get; set; } = new List<Breadcrumb>();
    }
}
