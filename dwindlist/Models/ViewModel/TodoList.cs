namespace dwindlist.Models.ViewModel
{
    public class TodoList
    {
        public List<TodoParent> Items { get; set; } = new List<TodoParent>();
        public int RootId { get; set; } = 0;
        public string Label { get; set; } = string.Empty;
        public List<Breadcrumb> Breadcrumbs { get; set; } = new List<Breadcrumb>();
    }

    public class TodoParent
    {
        public int? Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public char Status { get; set; } = 'i';
        public char Expanded { get; set; } = 'c';
        public List<TodoChild> Children { get; set; } = new List<TodoChild>();
    }

    public class TodoChild
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public char Status { get; set; } = 'i';
        public char Expanded { get; set; } = 'c';
    }

}
