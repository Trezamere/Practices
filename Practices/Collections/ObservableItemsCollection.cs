using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Practices.Collections
{
    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items are added, removed, updated, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class ObservableItemsCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        /// <summary>
        /// Flag which indicates if CollectionChangedNotifications are raised.
        /// </summary>
        private bool _suppressNotifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1"/> class.
        /// </summary>
        public ObservableItemsCollection()
        {
            _suppressNotifications = false;

            // NOTE: Do not expose the other overloaded constructors that accept a collection.
            // Otherwise we'd have to iterate the collection twice on initialization to add the PropertyChanged handlers.
            // Use the AddRange() method (not implemented, add when it becomes necessary to do bulk inserts and only notify once) instead.
        }

        #region Overrides

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            item.PropertyChanged += Item_PropertyChanged;
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            Items[index].PropertyChanged -= Item_PropertyChanged;

            base.RemoveItem(index);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param><param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, T item)
        {
            // We're replacing the item at index, so remove the handler.
            Items[index].PropertyChanged -= Item_PropertyChanged;

            // Verify the item we're adding does not already exist in the collection and have an Item_PropertyChanged handler attached.
            if (!Contains(item))
                item.PropertyChanged += Item_PropertyChanged;

            base.SetItem(index, item);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            // Need to clean up the PropertyChanged handlers before removing the items.
            foreach (var item in Items)
                item.PropertyChanged -= Item_PropertyChanged;

            base.ClearItems();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotifications)
                base.OnCollectionChanged(e);
        }

        #endregion

        /// <summary>
        /// Handler for when an item in the collection raises its <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Propogate the change notifications to the NotifyCollectionChanged notification.
            // Use the Replace action as it's the most indicative of a single item being modified, and will still allow .NET to listen and respond accordingly to CollectionChanged events.
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender));
        }

        /// <summary>
        /// Copies the elements from the specified collection to the end of the <see cref="ObservableItemsCollection{T}"/>.
        /// </summary>
        /// <param name="collection">The collection whose elements should be copied to the end of the <see cref="ObservableItemsCollection{T}"/>.</param>
        private void CopyFrom(IEnumerable<T> collection)
        {
            IList<T> items = Items;
            if (collection != null && items != null)
            {
                using (var enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
        }

        /// <summary>
        /// Copies the elements from the specified collection to the end of the <see cref="ObservableItemsCollection{T}"/>.
        /// </summary>
        /// <param name="collection">The collection whose elements should be copied to the end of the <see cref="ObservableItemsCollection{T}"/>. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            BeginUpdate();

            CopyFrom(collection);

            EndUpdate();
        }

        /// <summary>
        /// Prevents the collection from raising CollectionChanged events until the <see cref="EndUpdate"/> method is called.
        /// </summary>
        public void BeginUpdate()
        {
            // Disable change notifications.
            _suppressNotifications = true;
        }

        /// <summary>
        /// Resumes raising CollectionChanged events after being suspended by the <see cref="BeginUpdate"/> method.
        /// </summary>
        public void EndUpdate()
        {
            // Enable change notifications.
            _suppressNotifications = false;

            // Raise NotifyCollectionChangedAction.Reset as an indeterminate number of items were added/removed/updated.
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}