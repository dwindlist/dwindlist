using dwindlist.Data;
using dwindlist.Dtos;
using dwindlist.Models.ViewModel;

namespace dwindlist.Models.EntityManager;

public class TodoItemManager
{
    public TodoList GetTodoList(string userId, int? rootId)
    {
        if (rootId == null)
        {
            rootId = 0;
        }

        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var userItems = db.TodoItem.Where(i => i.UserId == userId);
            var parents = userItems.Where(i => i.ParentId == rootId).ToList();
            var todoList = new TodoList { RootId = (int)rootId };

            if (rootId != 0)
            {
                todoList.Label = userItems.Single(i => i.Id == rootId).Label;
            }

            foreach (var parent in parents)
            {
                var children = userItems.Where(i => i.ParentId == parent.Id).ToList();
                var sublist = new List<TodoChild>();

                foreach (var child in children)
                {
                    sublist.Add(new TodoChild
                    {
                        Id = child.Id,
                        Label = child.Label,
                        Status = child.Status,
                        Expanded = child.Expanded
                    });
                }

                todoList.Items.Add(new TodoParent
                {
                    Id = parent.Id,
                    Label = parent.Label,
                    Status = parent.Status,
                    Expanded = parent.Expanded,
                    Children = sublist
                });

            }

            return todoList;
        }
    }

    public void AddItem(string userId, int parentId, TodoItemDto todoItemDto)
    {
        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var newItem = new TodoItem
            {
                UserId = userId,
                Label = todoItemDto.Label,
                ParentId = parentId,
                Status = 'i',
            };

            db.TodoItem.Add(newItem);
            db.SaveChanges();
        }
    }

    public void UpdateItemLabel(string userId, int itemId, TodoItemDto todoItemDto)
    {
        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var userItems = db.TodoItem.Where(i => i.UserId == userId);
            var item = userItems.Single(i => i.Id == itemId);
            item.Label = todoItemDto.Label;

            db.SaveChanges();
        }
    }

    public void ToggleItemStatus(string userId, int itemId)
    {
        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var userItems = db.TodoItem.Where(i => i.UserId == userId);
            var item = userItems.Single(i => i.Id == itemId);
            item.Status = item.Status == 'i' ? 'c' : 'i';

            db.SaveChanges();
        }
    }

    public void ToggleItemExpanded(string userId, int itemId)
    {
        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var userItems = db.TodoItem.Where(i => i.UserId == userId);
            var item = userItems.Single(i => i.Id == itemId);
            item.Expanded = item.Expanded == 'c' ? 'e' : 'c';

            db.SaveChanges();
        }
    }
}
