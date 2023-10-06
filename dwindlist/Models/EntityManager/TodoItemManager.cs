using dwindlist.Data;
using dwindlist.Dtos;

namespace dwindlist.Models.EntityManager;

public class TodoItemManager
{
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
