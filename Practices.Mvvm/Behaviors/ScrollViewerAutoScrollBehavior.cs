using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Practices.Mvvm.Behaviors
{
    public class ScrollViewerAutoScrollBehavior : Behavior<ScrollViewer>
    {
        /// <summary>
        /// Gets or sets a value that determines how the <see cref="ScrollViewer"/> will scroll its content.
        /// <para>Defaults to <see cref="AutoScrollBehavior.ScrollToBottom"/>.</para>
        /// </summary>
        public AutoScrollBehavior ScrollingMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollViewerAutoScrollBehavior"/> class.
        /// </summary>
        public ScrollViewerAutoScrollBehavior()
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
                AddLoadedHandler(AssociatedObject);
            }
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
        /// Removes the handler to the specified <see cref="FrameworkElement"/>.
        /// </summary>
        private void RemoveLoadedHandler(FrameworkElement frameworkElement)
        {
            Contract.Requires(frameworkElement != null);

            frameworkElement.Loaded -= OnElementLoaded;
        }

        /// <summary>
        /// Handler for when the <see cref="FrameworkElement"/> is fully loaded.
        /// </summary>
        private void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            // Make sure we scroll when AssociatedObject is rendered for the first time and has content.
            ScrollCollection();
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
                RemoveLoadedHandler(AssociatedObject);
            }

            base.OnDetaching();
        }

        /// <summary>
        /// Scrolls the <see cref="Behavior.AssociatedObject"/> using the specified <see cref="ScrollingMode"/>.
        /// </summary>
        private void ScrollCollection()
        {
            if (AssociatedObject == null)
                return;

            switch (ScrollingMode)
            {
                case AutoScrollBehavior.ScrollToBottom:
                    AssociatedObject.ScrollToBottom();
                    break;
                case AutoScrollBehavior.ScrollToEnd:
                    AssociatedObject.ScrollToEnd();
                    break;
                case AutoScrollBehavior.ScrollToHome:
                    AssociatedObject.ScrollToHome();
                    break;
                case AutoScrollBehavior.ScrollToLeftEnd:
                    AssociatedObject.ScrollToLeftEnd();
                    break;
                case AutoScrollBehavior.ScrollToRightEnd:
                    AssociatedObject.ScrollToRightEnd();
                    break;
                case AutoScrollBehavior.ScrollToTop:
                    AssociatedObject.ScrollToTop();
                    break;
                default:
                    return;
            }
        }
    }
}