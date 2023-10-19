using dwindlist.Models.ViewModel;

namespace dwindlist.Models.EntityManager
{
    internal class BreadcrumbManager
    {
        private Dictionary<TodoItem, List<Breadcrumb>> Memo { get; set; } = new();

        public List<Breadcrumb> GetBreadcrumbs(List<TodoItem> list, int id)
        {
            List<Breadcrumb> breadcrumbs = new();

            if (id == 0)
            {
                return breadcrumbs;
            }

            TodoItem currentItem = list.Single(i => i.Id == id);

            while (currentItem.Id != 0)
            {
                if (Memo.ContainsKey(currentItem))
                {
                    breadcrumbs.AddRange(Memo[currentItem]);
                    break;
                }

                breadcrumbs.Add(new Breadcrumb { Id = currentItem.Id, Label = currentItem.Label });
                Memo.Add(currentItem, breadcrumbs);

                if (currentItem.ParentId == 0)
                {
                    break;
                }

                currentItem = list.Single(i => i.Id == currentItem.ParentId);
            }

            List<Breadcrumb> reversed = breadcrumbs.ToList();
            reversed.Reverse();
            return reversed;
        }
    }
}
