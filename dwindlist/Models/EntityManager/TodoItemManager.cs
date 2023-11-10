using dwindlist.Data;
using dwindlist.Dtos;
using dwindlist.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace dwindlist.Models.EntityManager
{
    /// <summary>
    /// Handles business logic and interfacing with the <see cref="ApplicationDbContext">DbContext</see>.
    /// </summary>
    /// <remarks>
    /// See <see cref="Controllers.TodoItemController">TodoItemController</see> for request handling and validation,
    /// which calls to objects of this class.
    /// </remarks>
    public class TodoItemManager
    {
        /// <summary>
        /// Gets the given <paramref name="userId"/>'s <see cref="TodoList"/> based on the <paramref name="rootId"/>.
        /// </summary>
        /// <param name="userId">Whose list to get</param>
        /// <param name="rootId">The root of the <see cref="TodoList"/></param>
        /// <returns>
        /// A <see cref="TodoList"/>.
        /// </returns>
        /// <remarks>
        /// The root is the parent of the <see cref="TodoList">ViewModel</see>'s outer list items.
        /// </remarks>
        /// <exception cref="ItemNotFoundException"/>
        /// <exception cref="ItemNotOwnedException"/>
        /// <exception cref="DbUpdateException"/>
        public TodoList GetTodoList(string userId, int rootId)
        {
            // a flag to disregard some exceptions
            // 0th item need not exist/have an owner
            bool topLevel = rootId == 0;

            using ApplicationDbContext db = new();

            // the focused item
            // its children will be parents in the TodoList
            TodoItem? rootItem = db.TodoItem.FirstOrDefault(
                i => (i.Id == rootId && i.Active == 'a')
            );

            bool itemExists = rootItem != null;
            if (!topLevel && !itemExists)
            {
                throw new ItemNotFoundException();
            }

            bool userOwnsItem = rootItem?.UserId == userId;
            if (!topLevel && !userOwnsItem)
            {
                throw new ItemNotOwnedException();
            }

            // convert to list immediately and perform subsequent queries
            // in-memory to prevent stressing the database.
            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            List<TodoItem> parents = userItems.Where(i => i.ParentId == rootId).ToList();
            TodoList todoList = new() { RootId = (int)rootId, };

            // get breadcrumbs if not top level (for navigation)
            if (!topLevel)
            {
                // disable compiler warning
                // we already checked for null above
                #pragma warning disable CS8602 // possibly null reference
                todoList.Label = rootItem.Label;
                #pragma warning restore CS8602 // possibly null reference

                todoList.Breadcrumbs = new BreadcrumbManager().GetBreadcrumbs(
                    userItems,
                    rootItem.ParentId
                );
            }

            // convert List<TodoItem> to TodoList.
            // TodoList is a ViewModel that more closely maps
            // to the concept of what's displayed in the frontend.
            foreach (TodoItem parent in parents)
            {
                // gather children
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

                // put into list
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

        /// <summary>
        /// Searches for <see cref="TodoItem">items</see> with matching labels.
        /// </summary>
        /// <param name="userId">Whose list to perform the search on.</param>
        /// <param name="pattern">Substring to search for.</param>
        /// <returns>
        /// A <see cref="FilteredList"/> with matching labels.
        /// </returns>
        /// <remarks>
        /// Matches are determined by substring.
        /// </remarks>
        public FilteredList SearchTodoList(string userId, string pattern)
        {
            // see FilterTodoList for actual implementation
            return FilterTodoList(
                userId,
                (list) =>
                {
                    return list.Where(i => i.Label.Contains(pattern)).ToList();
                }
            );
        }

        /// <summary>
        /// Filters for <see cref="TodoItem">items</see> based on their status.
        /// </summary>
        /// <param name="userId">Whose list to perform the filtering on.</param>
        /// <param name="completed">Filter by completed items or not.</param>
        /// <returns>
        /// A <see cref="FilteredList"/> filtered based on <paramref name="completed"/>.
        /// </returns>
        public FilteredList FilterTodoListByStatus(string userId, bool completed)
        {
            // see FilterTodoList for actual implementation
            return FilterTodoList(
                userId,
                (list) =>
                {
                    char status = completed ? 'c' : 'i';
                    return list.Where(i => i.Status == status).ToList();
                }
            );
        }

        /// <summary>
        /// Adds a new <see cref="TodoItem">item</see>.
        /// </summary>
        /// <param name="userId">Whose list to add the new <see cref="TodoItem">item</see> to.</param>
        /// <param name="parentId">The parent of which the item will be added to.</param>
        /// <param name="todoItemDto"><see cref="TodoItemDto">DTO</see> containing the new <see cref="TodoItem">item</see>'s label.</param>
        /// <exception cref="ItemNotFoundException"/>
        /// <exception cref="ItemNotOwnedException"/>
        /// <exception cref="DbUpdateException"/>
        public void AddItem(string userId, int parentId, TodoItemDto todoItemDto)
        {
            // a flag to disregard some exceptions
            // 0th item need not exist/have an owner
            bool topLevel = parentId == 0;

            using ApplicationDbContext db = new();

            // the item under which the new item will be added to
            TodoItem? parentItem = db.TodoItem.FirstOrDefault(
                i => i.Id == parentId && i.Active == 'a'
            );

            bool itemExists = parentItem != null;
            if (!topLevel && !itemExists)
            {
                throw new ItemNotFoundException();
            }

            bool userOwnsItem = parentItem?.UserId == userId;
            if (!topLevel && !userOwnsItem)
            {
                throw new ItemNotOwnedException();
            }

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

            // Update the status of parents
            // i.e., if it was already marked completed, unmark it
            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            _ = RecursiveUpdateParentStatus(newItem, userItems);
            _ = db.SaveChanges();
        }

        /// <summary>
        /// Updates the label of an <see cref="TodoItem">item</see>.
        /// </summary>
        /// <param name="userId">Whose list the <see cref="TodoItem">item</see> to be updated belongs to.</param>
        /// <param name="itemId">Id of the <see cref="TodoItem">item</see> to be updated.</param>
        /// <param name="todoItemDto"><see cref="TodoItemDto">DTO</see> containing the <see cref="TodoItem">item</see>'s updated label.</param>
        /// <exception cref="ItemNotFoundException"/>
        /// <exception cref="ItemNotOwnedException"/>
        /// <exception cref="DbUpdateException"/>
        public void UpdateItemLabel(string userId, int itemId, TodoItemDto todoItemDto)
        {
            using ApplicationDbContext db = new();

            TodoItem? item = db.TodoItem.FirstOrDefault(
                i => (i.Id == itemId && i.Active == 'a' )
            );

            bool itemExists = item != null;
            if (!itemExists)
            {
                throw new ItemNotFoundException();
            }

            bool userOwnsItem = item?.UserId == userId;
            if (!userOwnsItem)
            {
                throw new ItemNotOwnedException();
            }

            // disable compiler warning
            // we already checked for null above
            #pragma warning disable CS8602 // possibly null reference
            item.Label = todoItemDto.Label;
            #pragma warning restore CS8602 // possibly null reference
            _ = db.SaveChanges();
        }

        /// <summary>
        /// Expands or collapses an <see cref="TodoItem">item</see>.
        /// </summary>
        /// <param name="userId">Whose list the <see cref="TodoItem">item</see> to be expanded or collapsed belongs to.</param>
        /// <param name="itemId">Id of the <see cref="TodoItem">item</see> to be expanded or collapsed</param>
        /// <exception cref="ItemNotFoundException"/>
        /// <exception cref="ItemNotOwnedException"/>
        /// <exception cref="DbUpdateException"/>
        public void ToggleItemExpanded(string userId, int itemId)
        {
            using ApplicationDbContext db = new();
            TodoItem? item = db.TodoItem.FirstOrDefault(
                i => (i.Id == itemId && i.Active == 'a' )
            );

            bool itemExists = item != null;
            if (!itemExists)
            {
                throw new ItemNotFoundException();
            }

            bool userOwnsItem = item?.UserId == userId;
            if (!userOwnsItem)
            {
                throw new ItemNotOwnedException();
            }

            // disable compiler warning
            // we already checked for null above
            #pragma warning disable CS8602 // possibly null reference
            item.Expanded = item.Expanded == 'c' ? 'e' : 'c';
            #pragma warning restore CS8602 // possibly null reference

            _ = db.SaveChanges();
        }

        /// <summary>
        /// Toggles an <see cref="TodoItem">item</see> as complete or incomplete.
        /// </summary>
        /// <param name="userId">Whose list the <see cref="TodoItem">item</see> to be toggled complete or incomplete belongs to.</param>
        /// <param name="itemId">Id of the <see cref="TodoItem">item</see> to be toggled complete or incomplete</param>
        /// <exception cref="ItemNotFoundException"/>
        /// <exception cref="ItemNotOwnedException"/>
        /// <exception cref="DbUpdateException"/>
        public bool ToggleItemStatus(string userId, int itemId)
        {
            using ApplicationDbContext db = new();
            TodoItem? item = db.TodoItem.FirstOrDefault(
                i => (i.Id == itemId && i.Active == 'a' )
            );

            bool itemExists = item != null;
            if (!itemExists)
            {
                throw new ItemNotFoundException();
            }

            bool userOwnsItem = item?.UserId == userId;
            if (!userOwnsItem)
            {
                throw new ItemNotOwnedException();
            }

            // disable compiler warning
            // we already checked for null above
            #pragma warning disable CS8602 // possibly null reference
            char status = item.Status == 'i' ? 'c' : 'i';
            #pragma warning restore CS8602 // possibly null reference

            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            // update children
            Recurse(item, userItems, i => i.Status = status);

            // update parents (bool to tell the frontend to update)
            bool shouldUpdateParent = RecursiveUpdateParentStatus(item, userItems);

            _ = db.SaveChanges();
            return shouldUpdateParent;
        }

        /// <summary>
        /// Deletes an <see cref="TodoItem">item</see>.
        /// </summary>
        /// <param name="userId">Whose list the <see cref="TodoItem">item</see> to be deleted belongs to.</param>
        /// <param name="id">Id of the <see cref="TodoItem">item</see> to be deleted.</param>
        /// <remarks>
        /// <see cref="Models.TodoItem">Items</see> are not totally deleted; they are marked with a property that is ignored during queries.
        /// </remarks>
        /// <exception cref="ItemNotFoundException"/>
        /// <exception cref="ItemNotOwnedException"/>
        /// <exception cref="DbUpdateException"/>
        public bool DeleteItem(string userId, int id)
        {
            using ApplicationDbContext db = new();
            // mark item and its children as deleted
            TodoItem? item = db.TodoItem.FirstOrDefault(
                i => (i.Id == id && i.Active == 'a' )
            );

            bool itemExists = item != null;
            if (!itemExists)
            {
                throw new ItemNotFoundException();
            }

            bool userOwnsItem = item?.UserId == userId;
            if (!userOwnsItem)
            {
                throw new ItemNotOwnedException();
            }

            // disable compiler warning
            // we already checked for null above
            #pragma warning disable CS8602 // possibly null reference
            // mark deactivated as complete
            // updates assume userItems are all active, not deactivated
            // functionally the same in this context
            item.Active = 'd';
            #pragma warning restore CS8602 // possibly null reference

            // Update the status of parents
            // i.e., if it was the last incomplete item, mark its parent complete
            List<TodoItem> userItems = db.TodoItem
                .Where(i => i.UserId == userId)
                .Where(i => i.Active == 'a')
                .ToList();

            Recurse(item, userItems, i => i.Active = 'd');
            bool shouldUpdateParent = RecursiveUpdateParentStatus(item, userItems);
            _ = db.SaveChanges();
            return shouldUpdateParent;
        }

        /// <summary>
        /// Filters through the given user's list based on the given filter function.
        /// </summary>
        /// <param name="userId">Whose list to filter through</param>
        /// <param name="filter">Function that filters a list of <see cref="TodoItem"/>s</param>
        /// <returns>
        /// A <see cref="FilteredList"/>.
        /// </returns>
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

            // initialize outside the loop to take advantage of its memoization
            BreadcrumbManager bm = new();

            // convert List<TodoItem> to FilteredList
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

        /// <summary>
        /// Updates the children of a <see cref="TodoItem"/> based on the given operation.
        /// </summary>
        /// <param name="item">The <see cref="TodoItem"/> whose children will be updated</param>
        /// <param name="list">The list of all <see cref="TodoItem"/>s belonging to the user</param>
        /// <param name="operation">An action that modifies the <see cref="TodoItem"/>'s childrin</param>
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

        /// <summary>
        /// Updates the status of all the ancestors of a <see cref="TodoItem"/> if needed.
        /// </summary>
        /// <param name="item">The <see cref="TodoItem"/> whose ancestors should be updated.</param>
        /// <param name="userItems">The list of all <see cref="TodoItem"/>s belonging to the user</param>
        private static bool RecursiveUpdateParentStatus(TodoItem item, List<TodoItem> userItems)
        {
            // Because we'll recurse, we need to get the return value now.
            bool val = UpdateParentStatus(item, userItems);

            // Don't bother if this is a top level item.
            if (item.ParentId == 0)
            {
                return val;
            }

            // Loop up the ancestors as needed
            TodoItem currentItem = userItems.Single(i => i.Id == item.ParentId);
            bool shouldUpdateParent = true;
            while (shouldUpdateParent && currentItem.ParentId > 0)
            {
                shouldUpdateParent = UpdateParentStatus(currentItem, userItems);

                // If updates are no longer needed, or we've reached the top, stop
                if (!shouldUpdateParent || currentItem.ParentId == 0)
                {
                    break;
                }

                currentItem = userItems.Single(i => i.Id == currentItem.ParentId);
            }

            return val;
        }

        /// <summary>
        /// Updates the status the immediate parent of a <see cref="TodoItem"/> if needed.
        /// </summary>
        /// <param name="item">The <see cref="TodoItem"/> whose parent should be updated.</param>
        /// <param name="userItems">The list of all <see cref="TodoItem"/>s belonging to the user</param>
        private static bool UpdateParentStatus(TodoItem item, List<TodoItem> userItems)
        {
            // A flag that we'll toggle if changing this item should change the parent
            bool shouldUpdateParent = false;

            // Don't bother if this is a top level item.
            if (item.ParentId == 0)
            {
                return shouldUpdateParent;
            }

            // Save this for checking if we actually did toggle
            TodoItem parent = userItems.Single(i => i.Id == item.ParentId);
            char initParentStatus = parent.Status;

            // Make parent incomplete if we toggled an item incomplete
            if (item.Status == 'i')
            {
                parent.Status = 'i';
                shouldUpdateParent = item.Status != initParentStatus;
                return shouldUpdateParent;
            }

            // Complete parent if all children are complete
            List<TodoItem> children = userItems.Where(i => i.ParentId == item.ParentId).ToList();

            // Assume all children are complete
            bool shouldCompleteParent = true;
            foreach (TodoItem child in children)
            {
                // Toggle when assumption is wrong
                if (child.Status == 'i')
                {
                    shouldCompleteParent = false;
                    break;
                }
            }

            // Complete parent if needed
            if (shouldCompleteParent)
            {
                parent.Status = 'c';
            }

            // If we actually did change the parent,
            // we should check if that parent's parent should also be updated.
            shouldUpdateParent = shouldCompleteParent && initParentStatus == 'i';
            return shouldUpdateParent;
        }
    }

    /// <summary>
    /// The exception that is thrown when attempting to access
    /// a TodoItem that does not exist within the DbSet or IQueryable
    /// </summary>
    public class ItemNotFoundException : ArgumentException
    {
        public ItemNotFoundException() : base("Item does not exist.")
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when a user attempts to access
    /// a TodoItem that does not belong to them.
    /// </summary>
    public class ItemNotOwnedException : ArgumentException
    {
        public ItemNotOwnedException() : base("Item is not owned by user.")
        {
        }
    }
}
