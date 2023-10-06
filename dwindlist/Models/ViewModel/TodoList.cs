namespace dwindlist.Models.ViewModel;

public class TodoList
{
    public List<TodoParent> Items { get; set; } = new List<TodoParent>();
    public int RootId { get; set; } = 0;
}

public class TodoParent
{
    public int? Id { get; set; }
    public string Label { get; set; } = "";
    public List<TodoChild> Children { get; set; } = new List<TodoChild>();
}

public class TodoChild
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
}
