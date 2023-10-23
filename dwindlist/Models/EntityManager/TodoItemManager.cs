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
            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            List<TodoItem> parents = userItems.Where(i => i.ParentId == rootId).ToList();
            TodoList todoList = new() { RootId = (int)rootId, };

            if (rootId != 0)
            {
                TodoItem currentItem = userItems.Single(i => i.Id == rootId);
                todoList.Label = currentItem.Label;
                todoList.Breadcrumbs = new BreadcrumbManager().GetBreadcrumbs(
                    userItems,
                    currentItem.ParentId
                );
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

        public FilteredList SearchTodoList(string userId, string pattern)
        {
            return FilterTodoList(
                userId,
                (list) =>
                {
                    return list.Where(i => i.Label.Contains(pattern)).ToList();
                }
            );
        }

        public FilteredList FilterTodoListByStatus(string userId, bool completed)
        {
            return FilterTodoList(
                userId,
                (list) =>
                {
                    char status = completed ? 'c' : 'i';
                    return list.Where(i => i.Status == status).ToList();
                }
            );
        }

        public void AddItem(string userId, int parentId, TodoItemDto todoItemDto)
        {
            using ApplicationDbContext db = new();
            TodoItem newItem =
                new()
                {
                    UserId = userId,
                    Label = todoItemDto.Label,
                    ParentId = parentId,
                    Status = 'i',
                    Active = 'a',
                };

            _ = db.TodoItem.Add(newItem);
            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            _ = RecursiveUpdateParentStatus(newItem, userItems);
            _ = db.SaveChanges();
        }

        public void UpdateItemLabel(string userId, int itemId, TodoItemDto todoItemDto)
        {
            using ApplicationDbContext db = new();
            IQueryable<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a');

            TodoItem item = userItems.Single(i => i.Id == itemId);
            item.Label = todoItemDto.Label;

            _ = db.SaveChanges();
        }

        public void ToggleItemExpanded(string userId, int itemId)
        {
            using ApplicationDbContext db = new();
            IQueryable<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a');

            TodoItem item = userItems.Single(i => i.Id == itemId);
            item.Expanded = item.Expanded == 'c' ? 'e' : 'c';

            _ = db.SaveChanges();
        }

        public bool ToggleItemStatus(string userId, int itemId)
        {
            using ApplicationDbContext db = new();
            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            TodoItem item = userItems.Single(i => i.Id == itemId);
            char status = item.Status == 'i' ? 'c' : 'i';

            Recurse(item, userItems, i => i.Status = status);
            bool shouldUpdateParent = RecursiveUpdateParentStatus(item, userItems);
            _ = db.SaveChanges();
            return shouldUpdateParent;
        }

        public void DeleteItem(string userId, int id)
        {
            using ApplicationDbContext db = new();
            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            TodoItem item = userItems.Single(i => i.Id == id);
            Recurse(item, userItems, i => i.Active = 'd');

            _ = db.SaveChanges();
        }

        private static FilteredList FilterTodoList(
            string userId,
            Func<List<TodoItem>, List<TodoItem>> filter
        )
        {
            using ApplicationDbContext db = new();
            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            List<TodoItem> filteredItems = filter(userItems);
            FilteredList filteredList = new();
            BreadcrumbManager bm = new();

            foreach (TodoItem item in filteredItems)
            {
                filteredList.Items.Add(
                    new FilteredItem
                    {
                        Id = item.Id,
                        Label = item.Label,
                        Status = item.Status,
                        Breadcrumbs = bm.GetBreadcrumbs(userItems, item.ParentId)
                    }
                );
            }

            return filteredList;
        }

        private static void Recurse(TodoItem item, List<TodoItem> list, Action<TodoItem> operation)
        {
            Queue<TodoItem> queue = new();
            queue.Enqueue(item);

            while (queue.Count > 0)
            {
                TodoItem currentItem = queue.Dequeue();
                operation(currentItem);
                int currentId = currentItem.Id;
                list.Where(i => i.ParentId == currentId).ToList().ForEach(queue.Enqueue);
            }
        }

        private static bool RecursiveUpdateParentStatus(
            TodoItem item,
            List<TodoItem> userItems
        )
        {
            bool val = UpdateParentStatus(item, userItems);
            if (item.ParentId == 0)
            {
                return val;
            }

            TodoItem currentItem = userItems.Single(i => i.Id == item.ParentId);
            bool shouldUpdateParent = true;
            while (shouldUpdateParent && currentItem.ParentId > 0)
            {
                shouldUpdateParent = UpdateParentStatus(currentItem, userItems);
                if (!shouldUpdateParent || currentItem.ParentId == 0)
                {
                    break;
                }
                currentItem = userItems.Single(i => i.Id == currentItem.ParentId);
            }

            return val;
        }

        private static bool UpdateParentStatus(
            TodoItem item,
            List<TodoItem> userItems
        )
        {
            bool shouldUpdateParent = false;
            if (item.ParentId == 0)
            {
                return shouldUpdateParent;
            }

            // Make parent incomplete if we toggled an item incomplete
            TodoItem parent = userItems.Single(i => i.Id == item.ParentId);
            char initParentStatus = parent.Status;
            if (item.Status == 'i')
            {
                parent.Status = 'i';
                shouldUpdateParent = item.Status != initParentStatus;
                return shouldUpdateParent;
            }

            // Complete parent if all children are complete
            List<TodoItem> children = userItems.Where(i => i.ParentId == item.ParentId).ToList();

            bool shouldCompleteParent = true;
            foreach (TodoItem child in children)
            {
                if (child.Status == 'i')
                {
                    shouldCompleteParent = false;
                    break;
                }
            }

            if (shouldCompleteParent)
            {
                parent.Status = 'c';
            }

            shouldUpdateParent = shouldCompleteParent && initParentStatus == 'i';
            return shouldUpdateParent;
        }
    }
}
