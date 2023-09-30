using dwindlist.Dtos;
using dwindlist.Models.DB;

namespace dwindlist.Models.EntityManager;

public class TodoItemManager
{
    public int AddItem(TodoItemAddDto todoItemDto)
    {
        using (TodoItemDBContext db = new TodoItemDBContext())
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
