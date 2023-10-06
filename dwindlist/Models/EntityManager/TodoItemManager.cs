using dwindlist.Data;
using dwindlist.Dtos;
using dwindlist.Models.ViewModel;

namespace dwindlist.Models.EntityManager;

public class TodoItemManager
{
    public TodoList GetTodoList(string userId, int? parentId)
    {
        if (parentId == null)
        {
            parentId = 0;
        }

        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var parents = db.TodoItem.Where(i => i.ParentId == parentId && i.UserId == userId).ToList();

            var todoList = new TodoList();
            foreach (var parent in parents)
            {
                todoList.Items.Add(new TodoParent
                {
                    Id = parent.Id,
                    Label = parent.Label,
                });
            }

            return todoList;
        }
    }

    public int AddItem(TodoItemAddDto todoItemDto)
    {
        using (ApplicationDbContext db = new ApplicationDbContext())
        {
            var newItem = new TodoItem
            {
                UserId = todoItemDto.UserId,
                Label = todoItemDto.Label,
                ParentId = todoItemDto.ParentId,
                Status = todoItemDto.Status,
            };

            db.TodoItem.Add(newItem);
            db.SaveChanges();

            return newItem.Id;
        }
    }
}
