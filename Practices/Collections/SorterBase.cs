using System.Collections.Generic;
using System.ComponentModel;

namespace Practices.Collections
{
    public abstract class SorterBase<T> : Comparer<T>, ISorter
    {
        /// <summary>
        /// Gets the integer equivalent of the <see cref="Direction"/> property that will be used to modify the sort return value;
        /// </summary>
        protected int SortDirection
        {
            get
            {
                if (Direction == ListSortDirection.Descending)
                    return -1;

                return 1;
            }
        }

        /// <summary>
        /// Gets or sets the direction that will be used in the sort operations.
        /// </summary>
        public ListSortDirection Direction { get; set; }
    }
}