using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Practices.Mvvm.Extensions;

namespace Practices.Mvvm.Behaviors
{
	public class ItemsControlAutoScrollBehavior : Behavior<ItemsControl>
	{
		/// <summary>
		/// The <see cref="ScrollViewer"/> attached to the AssociatedObject.
		/// </summary>
		private ScrollViewer _scrollViewer;

        /// <summary>
        /// Gets or sets a value that determines how the <see cref="ScrollViewer"/> will scroll its content.
        /// <para>Defaults to <see cref="AutoScrollBehavior.ScrollToBottom"/>.</para>
        /// </summary>
        public AutoScrollBehavior ScrollingMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsControlAutoScrollBehavior"/> class.
        /// </summary>
	    public ItemsControlAutoScrollBehavior()
        {
            ScrollingMode = AutoScrollBehavior.ScrollToBottom;
        }

		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>
		/// Override this to hook up functionality to the AssociatedObject.
		/// </remarks>
		protected override void OnAttached()
		{
			base.OnAttached();

			if (AssociatedObject != null)
			{
				AddCollectionChangedHandler(AssociatedObject.Items);
			    AddLoadedHandler(AssociatedObject);
			}
		}

	    /// <summary>
		/// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
		/// </summary>
		/// <remarks>
		/// Override this to unhook functionality from the AssociatedObject.
		/// </remarks>
		protected override void OnDetaching()
		{
			if (AssociatedObject != null)
			{
				RemoveCollectionChangedHandler(AssociatedObject.Items);
			    RemoveLoadedHandler(AssociatedObject);

				_scrollViewer = null;
			}

			base.OnDetaching();
		}

		/// <summary>
		/// Adds the handler to the specified <see cref="INotifyCollectionChanged"/> implementation.
		/// </summary>
		private void AddCollectionChangedHandler(INotifyCollectionChanged sourceCollection)
		{
			Contract.Requires(sourceCollection != null);

			sourceCollection.CollectionChanged += OnCollectionChanged;
        }

        /// <summary>
        /// Adds the handler to the specified <see cref="FrameworkElement"/>.
        /// </summary>
        private void AddLoadedHandler(FrameworkElement frameworkElement)
        {
            Contract.Requires(frameworkElement != null);

            frameworkElement.Loaded += OnElementLoaded;
        }

		/// <summary>
        /// Removes the handler from the specified <see cref="INotifyCollectionChanged"/> implementation.
		/// </summary>
		private void RemoveCollectionChangedHandler(INotifyCollectionChanged sourceCollection)
		{
			Contract.Requires(sourceCollection != null);

			sourceCollection.CollectionChanged -= OnCollectionChanged;
		}

        /// <summary>
        /// Removes the handler to the specified <see cref="FrameworkElement"/>.
        /// </summary>
        private void RemoveLoadedHandler(FrameworkElement frameworkElement)
        {
            Contract.Requires(frameworkElement != null);

            frameworkElement.Loaded -= OnElementLoaded;
        }

	    /// <summary>
		/// Handler for when the <see cref="ItemsControl.Items"/> collection is modified.
		/// </summary>
		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
		    ScrollCollection();
		}

        /// <summary>
        /// Handler for when the <see cref="ItemsControl"/> is fully loaded.
        /// </summary>
        private void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            // Make sure we scroll when AssociatedObject is rendered for the first time and has content.
            ScrollCollection();
        }

        /// <summary>
        /// Scrolls the <see cref="Behavior.AssociatedObject"/> using the specified <see cref="ScrollingMode"/>.
        /// </summary>
	    private void ScrollCollection()
	    {
	        if (AssociatedObject == null)
	            return;
	        if (AssociatedObject.Items.Count == 0)
	            return;
	        if (GetScrollViewer() == null)
	            return;

	        switch (ScrollingMode)
	        {
	            case AutoScrollBehavior.ScrollToBottom:
	                _scrollViewer.ScrollToBottom();
	                break;
	            case AutoScrollBehavior.ScrollToEnd:
	                _scrollViewer.ScrollToEnd();
	                break;
	            case AutoScrollBehavior.ScrollToHome:
	                _scrollViewer.ScrollToHome();
	                break;
	            case AutoScrollBehavior.ScrollToLeftEnd:
	                _scrollViewer.ScrollToLeftEnd();
	                break;
	            case AutoScrollBehavior.ScrollToRightEnd:
	                _scrollViewer.ScrollToRightEnd();
	                break;
	            case AutoScrollBehavior.ScrollToTop:
	                _scrollViewer.ScrollToTop();
	                break;
	            default:
                    return;
	        }
	    }

	    /// <summary>
		/// Helper method which returns a <see cref="System.Windows.Controls.ScrollViewer"/>.
		/// </summary>
		private ScrollViewer GetScrollViewer()
		{
			// If our backing field has a scroll viewer, return it.
			// Otherwise search the visual tree for one, save it, and return.
            return _scrollViewer ?? (_scrollViewer = AssociatedObject.FindChild<ScrollViewer>());
		}
	}
}