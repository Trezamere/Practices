using System.Collections;
using System.ComponentModel;

namespace Practices.Collections
{
    /// <summary>
    /// Defines a method that a type implements to sort two objects.
    /// </summary>
    public interface ISorter : IComparer
    {
        /// <summary>
        /// Gets or sets the direction that will be used in the sort operations.
        /// </summary>
        ListSortDirection Direction { get; set; }
    }
}