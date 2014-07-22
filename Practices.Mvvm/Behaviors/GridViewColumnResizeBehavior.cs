using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Practices.Mvvm.Behaviors
{
	/// <summary>
	/// A <see cref="Behavior{T}"/> implementation which will automatically resize the <see cref="GridViewColumn"/>s marked as Auto to the new content.
	/// </summary>
	public class GridViewColumnResizeBehavior : Behavior<ListView>
	{
	    /// <summary>
		/// Listens for when the <see cref="ItemsControl.Items"/> collection changes.
		/// <para>Called after the behavior is attached to an AssociatedObject.</para>
		/// </summary>
		protected override void OnAttached()
		{
			base.OnAttached();

			var listView = AssociatedObject;
			if (listView == null)
				return;

			AddCollectionChangedHandler(listView.Items);
		    AddLoadedHandler(listView);
		}

	    /// <summary>
		/// Adds the Resize handler to the specified collection.
		/// </summary>
		private void AddCollectionChangedHandler(INotifyCollectionChanged sourceCollection)
		{
			Contract.Requires(sourceCollection != null);

			sourceCollection.CollectionChanged += OnListViewItemsCollectionChanged;
		}

        /// <summary>
        /// Adds the handler to the specified <see cref="FrameworkElement"/>.
        /// </summary>
        private void AddLoadedHandler(ListView listView)
        {
            Contract.Requires(listView != null);

            listView.Loaded += OnListViewLoaded;
        }

	    /// <summary>
		/// Removes the Resize handler from the specified collection.
		/// </summary>
		private void RemoveCollectionChangedHandler(INotifyCollectionChanged sourceCollection)
		{
			Contract.Requires(sourceCollection != null);

			sourceCollection.CollectionChanged -= OnListViewItemsCollectionChanged;
		}

        /// <summary>
        /// Removes the handler from the specified <see cref="FrameworkElement"/>.
        /// </summary>
        private void RemoveLoadedHandler(ListView listView)
        {
            Contract.Requires(listView != null);

            listView.Loaded -= OnListViewLoaded;
        }

		/// <summary>
		/// Handler that will loop through each <see cref="GridViewColumn"/> and automatically resize to the largest column item, if appropriate.
		/// </summary>
		private void OnListViewItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			var listView = AssociatedObject;
			if (listView == null || !listView.IsLoaded)
				return;

			var gridView = listView.View as GridView;
			if (gridView == null)
				return;

			MeasureGridViewColumns(gridView);
		}

        /// <summary>
        /// Handler for when the <see cref="ListView"/> is fully loaded.
        /// </summary>
        private void OnListViewLoaded(object sender, RoutedEventArgs e)
        {
            var listView = (ListView) sender;

            MeasureGridViewColumns((GridView) listView.View);
        }

	    /// <summary>
		/// Stops listening for when the <see cref="ItemsControl.Items"/> collection changes.
		/// <para>Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.</para>
		/// </summary>
		protected override void OnDetaching()
		{
			var listView = AssociatedObject;
	        if (listView != null)
	        {
	            RemoveCollectionChangedHandler(listView.Items);
	            RemoveLoadedHandler(listView);
	        }

	        base.OnDetaching();
		}

        /// <summary>
        /// Updates the Width of the specified <see cref="GridView"/> by measuring the width of it's individual columns.
        /// </summary>
        /// <param name="gridView">The <see cref="GridView"/> whose size will be measured.</param>
        private void MeasureGridViewColumns(GridView gridView)
        {
            double availableWidth = AssociatedObject.ActualWidth;
            var starColumns = new Dictionary<GridViewColumn,GridLength>();

            // Update the widths of all non-star columns and cache star columns for calculating at the end.
            foreach (var column in gridView.Columns)
            {
                // Check if the column has set the Width attached property.
                var width = GetWidth(column);
                if (width != default(GridLength))
                {
                    if (width.IsAbsolute)
                    {
                        column.Width = width.Value;
                    }
                    else if (width.IsAuto)
                    {
                        SetColumnWidthAuto(column);
                    }
                    else if (width.IsStar)
                    {
                        // Star columns will be calculated at the end.
                        starColumns.Add(column, width);

                        // For Star widths, add the column width (to nullify the subtraction later).
                        availableWidth += column.ActualWidth;
                    }
                }
                // GridLength attached property width not set
                // If Column.Width set to Auto, make sure it's re-evaluated on CollectionChanged
                else if (Double.IsNaN(column.Width))
                {
                    SetColumnWidthAuto(column);
                }

                // Subtract the adjusted column width from the total available width.
                availableWidth -= column.ActualWidth;
            }

            // We now have the remaining available space in the Grid, calculate the StarColumn widths.
            MeasureStarColumns(starColumns, Math.Floor(availableWidth));
        }

        /// <summary>
        /// Helper method which will calculate the appropriate column widths based on the star-syntax values.
        /// </summary>
        /// <param name="starColumns">The collection of columns and their width.</param>
        /// <param name="availableWidth">The available width which will be distributed among the remaining columns.</param>
	    private void MeasureStarColumns(Dictionary<GridViewColumn, GridLength> starColumns, double availableWidth)
	    {
            if (starColumns.Count == 0 || availableWidth <= 0)
                return;

            // Using the following algorithm (not sure if this is what .NET is doing for grids, probably not)
            // Divide the available space evenly among the # of star columns, then multiply even distributed weight for each column by its * value weight
            // (Available Space / # Star Columns) * Star Column Width Weight.
            // Example:
            //  Theres 1200 pixels available with 4 columns set to 1*, 1*, .5* and 1.5*
            //  The columns will have respective widths of 300, 300, 150, and 450

            var distributedColumnWidth = availableWidth / starColumns.Count;
	        foreach (var starColumn in starColumns)
	        {
	            var column = starColumn.Key;
	            var gridLength = starColumn.Value;
                
	            column.Width = distributedColumnWidth * gridLength.Value;
	        }
	    }

	    /// <summary>
        /// Helper method which will set the specified columns width to Auto after forcing the column to re-measure its desired size.
        /// </summary>
        /// <param name="column">The column whose width will be set to Auto.</param>
	    private void SetColumnWidthAuto(GridViewColumn column)
	    {
	        column.Width = column.ActualWidth;
	        column.Width = Double.NaN;
	    }

	    #region Width Attached Property
        
        /// <summary>
        /// Identifies the <see cref="Width"/> attached property.
        /// </summary>
	    public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
	        "Width", typeof (GridLength), typeof (GridViewColumnResizeBehavior), new PropertyMetadata(default(GridLength)));

        /// <summary>
        /// Sets the value of the <see cref="Width"/> attached property for a specified dependency object.
        /// <para>Allows for setting <see cref="GridViewColumn"/> widths using Grid-like syntax (including <see cref="GridUnitType.Star"/> notation).</para>
        /// </summary>
        /// <param name="element">The dependency object for which to set the value of the <see cref="Width"/> property.</param>
        /// <param name="value">The new value to set the property to.</param>
	    public static void SetWidth(DependencyObject element, GridLength value)
	    {
	        element.SetValue(WidthProperty, value);
	    }

        /// <summary>
        /// Returns the value of the <see cref="Width"/> attached property for a specified dependency object.
        /// </summary>
        /// <param name="element">The dependency object for which to retrieve the value of the <see cref="Width"/> property.</param>
        /// <returns>The current value of the <see cref="Width"/> attached property on the specified dependency object.</returns>
	    public static GridLength GetWidth(DependencyObject element)
	    {
	        return (GridLength) element.GetValue(WidthProperty);
	    }

	    #endregion
	}
}