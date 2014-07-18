using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace baokit.Common.Wpf.Behaviors
{
    /// <summary>
    /// A <see cref="Behavior{T}"/> implementation which will automatically sort a <see cref="ListView"/>.
    /// </summary>
    /// 
    /// <remarks>
    /// We use <see cref="ICollectionView"/>'s to manage the sorting.
    /// http://drwpf.com/blog/2008/10/20/itemscontrol-e-is-for-editable-collection/ is a great article on the various options we have with respect to optimizing the performance of the ICollectionView.
    /// </remarks>
    public class GridViewColumnSortBehavior : Behavior<ListView>
    {
        /// <summary>
        /// Field indicating the currently sorted column header.
        /// </summary>
        private GridViewColumnHeader _sortedColumnHeader;

        #region Attaching Logic

        /// <summary>
        /// Listens for when the <see cref="GridViewColumnHeader"/>s are clicked.
        /// <para>Called after the behavior is attached to an AssociatedObject.</para>
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            var listView = AssociatedObject;
            if (listView == null)
                return;

            AddClickHandler(listView);
            //AddCollectionChangedHandler(listView.Items);
            //AddLoadedHandler(listView);
        }

        /// <summary>
        /// Adds the handler to the specified <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        private void AddCollectionChangedHandler(INotifyCollectionChanged sourceCollection)
        {
            Contract.Requires(sourceCollection != null);

            sourceCollection.CollectionChanged += OnListViewItemsCollectionChanged;
        }

        /// <summary>
        /// Adds the handler to the specified <see cref="ListView"/>.
        /// </summary>
        private void AddClickHandler(ListView listView)
        {
            listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnGridViewColumnHeaderClicked));
        }

        /// <summary>
        /// Adds the handler to the specified <see cref="FrameworkElement"/>.
        /// </summary>
        private void AddLoadedHandler(ListView listView)
        {
            Contract.Requires(listView != null);

            listView.Loaded += OnListViewLoaded;
        }

        #endregion

        #region Sorting Logic

        /// <summary>
        /// Handler for when the <see cref="GridViewColumnHeader"/> is clicked.
        /// </summary>
        private void OnGridViewColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            var columnHeader = e.OriginalSource as GridViewColumnHeader;
            if (columnHeader != null && columnHeader.Column != null &&
                columnHeader.Role != GridViewColumnHeaderRole.Padding)
            {
                // Get the ICollectionView
                var collectionView = CollectionViewSource.GetDefaultView(AssociatedObject.ItemsSource);
                ApplySort(collectionView, columnHeader);
            }
        }

        /// <summary>
        /// Adds or updates the sorting logic on the specified <see cref="ICollectionView"/>.
        /// </summary>
        /// <param name="collectionView">The CollectionView being sorted.</param>
        /// <param name="columnHeader">The specific column in the CollectionView being sorted.</param>
        private void ApplySort(ICollectionView collectionView, GridViewColumnHeader columnHeader)
        {
            var customComparer = GetCustomSort(columnHeader.Column);
            var propertyName = GetPropertyName(columnHeader.Column);

            // If no sorting mechanisms were defined on the column, cannot sort the column, so return.
            if (customComparer == null && propertyName == null)
                return;

            // Determine the sorting direction that will be used.
            var sortDirection = GetSortDirection(columnHeader);
            if (ReferenceEquals(_sortedColumnHeader, columnHeader))
            {
                // The column is already being sorted, flip the directions.
                sortDirection = (sortDirection == ListSortDirection.Ascending)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;

                // Update our attached property.
                SetSortDirection(columnHeader, sortDirection);
            }

            // Update the sort glyph if enabled.
            if (GetShowSortGlyph(AssociatedObject))
            {
                if (_sortedColumnHeader != null)
                {
                    // If a column is already being sorted, remove the sort glyph on the old column.
                    RemoveSortGlyph(_sortedColumnHeader);
                }

                AddSortGlyph(columnHeader, sortDirection, null);
            }

            if (collectionView is ListCollectionView && customComparer != null)
            {
                // If a custom IComparer was specified, sort using the faster IComparer
                ApplyCustomComparer((ListCollectionView) collectionView, customComparer, sortDirection);
            }
            else
            {
                // Otherwise default to SortDescriptions
                if (!String.IsNullOrWhiteSpace(propertyName))
                {
                    ApplySortDescription(collectionView, propertyName, sortDirection);
                }
            }

            // lastly, update our sorted column.
            _sortedColumnHeader = columnHeader;
        }

        /// <summary>
        /// Adds or updates sort logic on the specified <see cref="ListCollectionView"/> to use the specified <see cref="IComparer"/> to sort the columns.
        /// </summary>
        /// <param name="collectionView">The CollectionView being sorted.</param>
        /// <param name="customComparer">The comparer that will be sorting the rows.</param>
        /// <param name="sortDirection">The direction the CustomComparer will sort.</param>
        private void ApplyCustomComparer(ListCollectionView collectionView, IComparer customComparer, ListSortDirection sortDirection)
        {
            var sorter = customComparer as ISorter;
            if (sorter != null)
                sorter.Direction = sortDirection;

            // Setting this property causes an immediate refresh unless a DeferRefresh is in effect.
            // Setting this property clears a previously set SortDescriptions value.
            collectionView.CustomSort = customComparer;
        }

        /// <summary>
        /// Adds or updates sort logic on the specified <see cref="ICollectionView"/> to use the built-in <see cref="SortDescription"/> structures.
        /// </summary>
        /// <param name="view">The CollectionView being sorted.</param>
        /// <param name="propertyName">The name of the property being sorted.</param>
        /// <param name="sortDirection">The sort order.</param>
        private void ApplySortDescription(ICollectionView view, string propertyName, ListSortDirection sortDirection)
        {
            // Do not refresh until updating sorting logic is complete
            using (view.DeferRefresh())
            {
                // Can only sort one column at a time.
                if (view.SortDescriptions.Count > 0)
                    view.SortDescriptions.Clear();

                // Add the new sort descriptor
                view.SortDescriptions.Add(new SortDescription(propertyName, sortDirection));
            }
        }

        #endregion

        /// <summary>
        /// Handler for when the <see cref="ListView"/> is fully loaded.
        /// </summary>
        private void OnListViewLoaded(object sender, RoutedEventArgs e)
        {
            var listView = (ListView) sender;
            // TODO: If functionality is added to the behavior to manage which columns load sorted, do it here.
        }

        /// <summary>
        /// Handler for when the <see cref="INotifyCollectionChanged"/> is updated.
        /// </summary>
        private void OnListViewItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: You can tell the ICollectionView to update individual items only via the IEditableCollectionView interface, do that here to improve performance by updating individual items only instead of the entire view.
            // Note: Currently for the CotCollectionWindow we're just exploiting the fact that the underlying collection raises NotifyCollectionChangedAction.Replace on an item update.
            // This has a known side-effect that if the item that is updated is the currently selected item, that selection state gets cleared away.  You can get around this by using the IEditableCollectionView interface.
        }

        #region Detaching Logic

        /// <summary>
        /// Stops listening for when the <see cref="ItemsControl.Items"/> collection changes.
        /// <para>Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.</para>
        /// </summary>
        protected override void OnDetaching()
        {
            var listView = AssociatedObject;
            if (listView != null)
            {
                RemoveClickHandler(listView);
                //RemoveCollectionChangedHandler(listView.Items);
                //RemoveLoadedHandler(listView);
            }

            base.OnDetaching();
        }

        /// <summary>
        /// Removes the handler from the specified <see cref="ListView"/>.
        /// </summary>
        /// <param name="listView"></param>
        private void RemoveClickHandler(ListView listView)
        {
            Contract.Requires(listView != null);

            listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnGridViewColumnHeaderClicked));
        }

        /// <summary>
        /// Removes the handler from the specified <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        private void RemoveCollectionChangedHandler(INotifyCollectionChanged sourceCollection)
        {
            Contract.Requires(sourceCollection != null);

            sourceCollection.CollectionChanged -= OnListViewItemsCollectionChanged;
        }

        /// <summary>
        /// Removes the handler from the specified <see cref="ListView"/>.
        /// </summary>
        private void RemoveLoadedHandler(ListView listView)
        {
            Contract.Requires(listView != null);

            listView.Loaded -= OnListViewLoaded;
        }

        #endregion

        #region SortDirection attached property

        /// <summary>
        /// Identifies the <see cref="SortDirection"/> attached property.
        /// </summary>
        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached(
            "SortDirection", typeof (ListSortDirection), typeof (GridViewColumnSortBehavior),
            new PropertyMetadata(default(ListSortDirection)));

        /// <summary>
        /// Sets the value of the <see cref="SortDirection"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to set the value of the <see cref="SortDirection"/> property.</param>
        /// <param name="value">The new value to set the property to.</param>
        public static void SetSortDirection(DependencyObject element, ListSortDirection value)
        {
            element.SetValue(SortDirectionProperty, value);
        }

        /// <summary>
        /// Returns the value of the <see cref="SortDirection"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to retrieve the value of the <see cref="SortDirection"/> property.</param>
        /// <returns>The current value of the <see cref="SortDirection"/> attached property on the specified dependency object.</returns>
        public static ListSortDirection GetSortDirection(DependencyObject element)
        {
            return (ListSortDirection) element.GetValue(SortDirectionProperty);
        }

        #endregion

        #region CustomSort attached property

        /// <summary>
        /// Identifies the <see cref="CustomSort"/> attached property.
        /// </summary>
        public static readonly DependencyProperty CustomSortProperty = DependencyProperty.RegisterAttached(
            "CustomSort", typeof (IComparer), typeof (GridViewColumnSortBehavior),
            new PropertyMetadata(default(IComparer)));

        /// <summary>
        /// Sets the value of the <see cref="CustomSort"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to set the value of the <see cref="CustomSort"/> property.</param>
        /// <param name="value">The new value to set the property to.</param>
        public static void SetCustomSort(DependencyObject element, IComparer value)
        {
            element.SetValue(CustomSortProperty, value);
        }

        /// <summary>
        /// Returns the value of the <see cref="CustomSort"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to retrieve the value of the <see cref="CustomSort"/> property.</param>
        /// <returns>The current value of the <see cref="CustomSort"/> attached property on the specified dependency object.</returns>
        public static IComparer GetCustomSort(DependencyObject element)
        {
            return (IComparer) element.GetValue(CustomSortProperty);
        }

        #endregion

        #region PropertyName attached property

        /// <summary>
        /// Identifies the <see cref="PropertyName"/> attached property.
        /// </summary>
        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached(
            "PropertyName", typeof (string), typeof (GridViewColumnSortBehavior), new PropertyMetadata(default(string)));

        /// <summary>
        /// Sets the value of the <see cref="PropertyName"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to set the value of the <see cref="PropertyName"/> property.</param>
        /// <param name="value">The new value to set the property to.</param>
        public static void SetPropertyName(DependencyObject element, string value)
        {
            element.SetValue(PropertyNameProperty, value);
        }

        /// <summary>
        /// Returns the value of the <see cref="PropertyName"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to retrieve the value of the <see cref="PropertyName"/> property.</param>
        /// <returns>The current value of the <see cref="PropertyName"/> attached property on the specified dependency object.</returns>
        public static string GetPropertyName(DependencyObject element)
        {
            return (string) element.GetValue(PropertyNameProperty);
        }

        #endregion

        #region Sorting Glyph logic

        /// <summary>
        /// Adds a <see cref="SortGlyphAdorner"/> to the specified <see cref="GridViewColumnHeader"/>.
        /// </summary>
        /// <param name="columnHeader">The column that will be decorated with a sorting glyph.</param>
        /// <param name="direction">The direction of the sort.</param>
        /// <param name="sortGlyph">The image indicating the column is being sorted in a specific direction.</param>
        private void AddSortGlyph(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            adornerLayer.Add(new SortGlyphAdorner(columnHeader, direction, sortGlyph));
        }

        /// <summary>
        /// Removes the <see cref="SortGlyphAdorner"/> from the specified <see cref="GridViewColumnHeader"/>.
        /// </summary>
        /// <param name="columnHeader">The column with a sorting glyph that will be removed.</param>
        private void RemoveSortGlyph(GridViewColumnHeader columnHeader)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            Adorner[] adorners = adornerLayer.GetAdorners(columnHeader);
            if (adorners != null)
            {
                foreach (var adorner in adorners.OfType<SortGlyphAdorner>())
                {
                    adornerLayer.Remove(adorner);
                }
            }
        }

        #region ShowSortGlyph attached property

        /// <summary>
        /// Identifies the <see cref="ShowSortGlyph"/> attached property.
        /// </summary>
        public static readonly DependencyProperty ShowSortGlyphProperty = DependencyProperty.RegisterAttached(
            "ShowSortGlyph", typeof (bool), typeof (GridViewColumnSortBehavior), new UIPropertyMetadata(true));

        /// <summary>
        /// Sets the value of the <see cref="ShowSortGlyph"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to set the value of the <see cref="ShowSortGlyph"/> property.</param>
        /// <param name="value">The new value to set the property to.</param>
        public static void SetShowSortGlyph(DependencyObject element, bool value)
        {
            element.SetValue(ShowSortGlyphProperty, value);
        }

        /// <summary>
        /// Returns the value of the <see cref="ShowSortGlyph"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to retrieve the value of the <see cref="ShowSortGlyph"/> property.</param>
        /// <returns>The current value of the <see cref="ShowSortGlyph"/> attached property on the specified dependency object.</returns>
        public static bool GetShowSortGlyph(DependencyObject element)
        {
            return (bool) element.GetValue(ShowSortGlyphProperty);
        }

        #endregion

        #region SortGlyphAdorner nested class

        /// <summary>
        /// Decorates a <see cref="GridViewColumnHeader"/> with an arrow indicating the sort direction.
        /// </summary>
        private class SortGlyphAdorner : Adorner
        {
            /// <summary>
            /// The column header being adorned.
            /// </summary>
            private readonly GridViewColumnHeader _columnHeader;

            /// <summary>
            /// The direction the sorting glyph will be indicating.
            /// </summary>
            private readonly ListSortDirection _direction;

            /// <summary>
            /// The image indicating the column is being sorted in a specific direction.
            /// </summary>
            private readonly ImageSource _sortGlyph;

            /// <summary>
            /// Initializes a new instance of the <see cref="SortGlyphAdorner"/> class.
            /// </summary>
            /// <param name="columnHeader">The column header that will be adorned.</param>
            /// <param name="direction">The direction of the sort.</param>
            /// <param name="sortGlyph">The image indicating the column is being sorted in a specific direction.</param>
            public SortGlyphAdorner(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph) : base(columnHeader)
            {
                _columnHeader = columnHeader;
                _direction = direction;
                _sortGlyph = sortGlyph;
            }

            /// <summary>
            /// Gets the default shape used to indicate a column is being sorted.
            /// </summary>
            /// <returns>the geometry indicating a sort direction.</returns>
            private Geometry GetDefaultGlyph()
            {
                double x1 = _columnHeader.ActualWidth - 13;
                double x2 = x1 + 10;
                double x3 = x1 + 5;
                double y1 = _columnHeader.ActualHeight/2 - 3;
                double y2 = y1 + 5;

                if (_direction == ListSortDirection.Ascending)
                {
                    double tmp = y1;
                    y1 = y2;
                    y2 = tmp;
                }

                var pathSegmentCollection = new PathSegmentCollection
                    {
                        new LineSegment(new Point(x2, y1), true),
                        new LineSegment(new Point(x3, y2), true)
                    };

                var pathFigure = new PathFigure(new Point(x1, y1), pathSegmentCollection, true);

                var pathFigureCollection = new PathFigureCollection {pathFigure};

                var pathGeometry = new PathGeometry(pathFigureCollection);

                return pathGeometry;
            }

            /// <summary>
            /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing. 
            /// </summary>
            /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (_sortGlyph != null)
                {
                    double x = _columnHeader.ActualWidth - 13;
                    double y = _columnHeader.ActualHeight/2 - 5;
                    Rect rect = new Rect(x, y, 10, 10);
                    drawingContext.DrawImage(_sortGlyph, rect);
                }
                else
                {
                    drawingContext.DrawGeometry(new SolidColorBrush(Colors.LightGray) {Opacity = 0.5},
                        new Pen(Brushes.Gray, 0.5), GetDefaultGlyph());
                }
            }
        }

        #endregion

        #endregion
    }
}