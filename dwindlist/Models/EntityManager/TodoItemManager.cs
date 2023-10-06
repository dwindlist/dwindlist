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
                        Status = child.Status
                    });
                }

                todoList.Items.Add(new TodoParent
                {
                    Id = parent.Id,
                    Label = parent.Label,
                    Status = parent.Status,
                    Children = sublist
                });

            }

            return todoList;
        }
    }

    public void AddItem(string userId, TodoItemAddDto todoItemDto)
    {
        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var newItem = new TodoItem
            {
                UserId = userId,
                Label = todoItemDto.Label,
                ParentId = todoItemDto.ParentId,
                Status = todoItemDto.Status,
            };

            db.TodoItem.Add(newItem);
            db.SaveChanges();
        }
    }

    public void UpdateItemLabel(string userId, int itemId, TodoItemUpdateDto todoItemDto)
    {
        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var userItems = db.TodoItem.Where(i => i.UserId == userId);
            var item = userItems.Single(i => i.Id == itemId);
            item.Label = todoItemDto.Label;

            db.SaveChanges();
        }
    }

    public void ToggleItem(string userId, int itemId)
    {
        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var userItems = db.TodoItem.Where(i => i.UserId == userId);
            var item = userItems.Single(i => i.Id == itemId);
            item.Status = item.Status == 'i' ? 'c' : 'i';

            db.SaveChanges();
        }
    }
}
