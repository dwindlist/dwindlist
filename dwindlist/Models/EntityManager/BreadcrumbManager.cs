using dwindlist.Models.ViewModel;

namespace dwindlist.Models.EntityManager
{
    /// <summary>
    /// A helper class to more efficiently get <see cref="Breadcrumb"/>s (A list of ancestors).
    /// </summary>
    internal class BreadcrumbManager
    {
        /// <summary>
        /// A store for previously encountered <see cref="Breadcrumb"/>s.
        /// </summary>
        private Dictionary<TodoItem, List<Breadcrumb>> Memo { get; set; } = new();

        /// <summary>
        /// Gets the <see cref="Breadcrumb"/>s of a given <see cref="TodoItem"/>.
        /// </summary>
        /// <param name="list">The list of all <see cref="TodoItem"/>s belonging to the user.</param>
        /// <param name="id">The id of the <see cref="TodoItem"/> whose ancestors are being searched for.</param>
        public List<Breadcrumb> GetBreadcrumbs(List<TodoItem> list, int id)
        {
            List<Breadcrumb> breadcrumbs = new();

            // Don't bother if this is a top level item
            if (id == 0)
            {
                return breadcrumbs;
            }

            TodoItem currentItem = list.Single(i => i.Id == id);

            while (currentItem.Id != 0)
            {
                // use memo if the memo has it
                if (Memo.ContainsKey(currentItem))
                {
                    breadcrumbs.AddRange(Memo[currentItem]);
                    break;
                }

                // otherwise, search for it and add it to the memo
                breadcrumbs.Add(new Breadcrumb { Id = currentItem.Id, Label = currentItem.Label });
                Memo.Add(currentItem, breadcrumbs);

                // quit if we're at the top
                if (currentItem.ParentId == 0)
                {
                    break;
                }

                currentItem = list.Single(i => i.Id == currentItem.ParentId);
            }

            // searching up means the deeper items are first (child > parent > root)
            // breadcrumbs should be the other way round (root > parent > child)
            List<Breadcrumb> reversed = breadcrumbs.ToList();
            reversed.Reverse();
            return reversed;
        }
    }
}
