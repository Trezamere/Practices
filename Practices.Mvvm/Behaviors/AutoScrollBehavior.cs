using System.Windows.Controls;

namespace baokit.Common.Wpf.Behaviors
{
    /// <summary>
    /// Enumeration of the possible methods a <see cref="ScrollViewer"/> can use to automatically scroll its content.
    /// </summary>
    public enum AutoScrollBehavior
    {
        /// <summary>
        /// Will not automatically scroll the content.
        /// </summary>
        None,
        /// <summary>
        /// Scrolls vertically to the end of the content.
        /// </summary>
        ScrollToBottom,
        /// <summary>
        /// Scrolls to both the vertical and horizontal end points of the content.
        /// </summary>
        ScrollToEnd,
        /// <summary>
        /// Scrolls vertically and horizontally to the beginning of the content.
        /// </summary>
        ScrollToHome,
        /// <summary>
        /// Scrolls horizontally to the beginning of the content.
        /// </summary>
        ScrollToLeftEnd,
        /// <summary>
        /// Scrolls horizontally to the end of the content.
        /// </summary>
        ScrollToRightEnd,
        /// <summary>
        /// Scrolls vertically to the beginning of the content.
        /// </summary>
        ScrollToTop,
    }
}