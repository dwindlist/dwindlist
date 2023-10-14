using dwindlist.Data;
using dwindlist.Dtos;
using dwindlist.Models.ViewModel;

namespace dwindlist.Models.EntityManager
{
    public class TodoItemManager
    {
        public TodoList GetTodoList(string userId, int? rootId)
        {
            rootId ??= 0;

            using ApplicationDbContext db = new();
            IQueryable<TodoItem> userItems = db.TodoItem.Where(i => i.UserId == userId);
            List<TodoItem> parents = userItems.Where(i => i.ParentId == rootId).ToList();
            TodoList todoList = new() { RootId = (int)rootId };

            if (rootId != 0)
            {
                TodoItem currentItem = userItems.Single(i => i.Id == rootId);
                todoList.Label = currentItem.Label;
                while (currentItem.ParentId != 0)
                {
                    currentItem = userItems.Single(i => i.Id == currentItem.ParentId);
                    todoList.Breadcrumbs.Insert(
                        0,
                        new Breadcrumb { Id = currentItem.Id, Label = currentItem.Label }
                    );
                }
            }

            foreach (TodoItem parent in parents)
            {
                List<TodoItem> children = userItems.Where(i => i.ParentId == parent.Id).ToList();
                List<TodoChild> sublist = new();

                foreach (TodoItem child in children)
                {
                    sublist.Add(
                        new TodoChild
                        {
                            Id = child.Id,
                            Label = child.Label,
                            Status = child.Status,
                            Expanded = child.Expanded
                        }
                    );
                }

                todoList.Items.Add(
                    new TodoParent
                    {
                        Id = parent.Id,
                        Label = parent.Label,
                        Status = parent.Status,
                        Expanded = parent.Expanded,
                        Children = sublist
                    }
                );
            }

            return todoList;
        }

        public void AddItem(string userId, int parentId, TodoItemDto todoItemDto)
        {
            using ApplicationDbContext db = new();
            TodoItem newItem = new()
            {
                UserId = userId,
                Label = todoItemDto.Label,
                ParentId = parentId,
                Status = 'i',
            };

            _ = db.TodoItem.Add(newItem);
            _ = db.SaveChanges();
        }

        public void UpdateItemLabel(string userId, int itemId, TodoItemDto todoItemDto)
        {
            using ApplicationDbContext db = new();
            IQueryable<TodoItem> userItems = db.TodoItem.Where(i => i.UserId == userId);
            TodoItem item = userItems.Single(i => i.Id == itemId);
            item.Label = todoItemDto.Label;

            _ = db.SaveChanges();
        }

        public void ToggleItemStatus(string userId, int itemId)
        {
            using ApplicationDbContext db = new();
            IQueryable<TodoItem> userItems = db.TodoItem.Where(i => i.UserId == userId);
            TodoItem item = userItems.Single(i => i.Id == itemId);
            item.Status = item.Status == 'i' ? 'c' : 'i';

            _ = db.SaveChanges();
        }

        public void ToggleItemExpanded(string userId, int itemId)
        {
            using ApplicationDbContext db = new();
            IQueryable<TodoItem> userItems = db.TodoItem.Where(i => i.UserId == userId);
            TodoItem item = userItems.Single(i => i.Id == itemId);
            item.Expanded = item.Expanded == 'c' ? 'e' : 'c';

            _ = db.SaveChanges();
        }
    }
}
