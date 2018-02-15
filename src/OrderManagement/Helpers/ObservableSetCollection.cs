using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OrderManagement
{
    public class ObservableSetCollection<T> : ObservableCollection<T>
    {
        public ObservableSetCollection(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Append(item);
            }
        }

        public ObservableSetCollection()
        {

        }

        public void Append(T item)
        {
            if (Contains(item))
                return;
            base.Add(item);
        }

        protected override void InsertItem(int index, T item)
        {
            if (Contains(item))
                return;
            base.InsertItem(index, item);
        }
    }
}