using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Practices.Mvvm.Extensions
{
    /// <summary>
    /// Contains the DependencyObject extension methods.
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Finds the nearest child of an element of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the child to find.</typeparam>
        /// <param name="source">The root element that marks the source of the search.</param>
        /// <returns>The nearest child of <paramref name="source"/> matching the specified type; if no such child exists, null is returned</returns>
        public static T FindChild<T>(this DependencyObject source) where T : DependencyObject
        {
            if (!(source is Visual || source is Visual3D))
                throw new ArgumentException("The value of element must represent either a Visual or Visual3D object.");

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(source); i++)
            {
                var child = VisualTreeHelper.GetChild(source, i);

                var result = (child as T) ?? FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }  

        /// <summary>
        /// Finds the nearest parent of an element of a specified type. A maximum depth to search can optionally be specified.
        /// </summary>
        /// <typeparam name="T">The type of the parent to find.</typeparam>
        /// <param name="source">The root element that marks the source of the search.</param>
        /// <param name="maxDepth">The maximum depth to walk the tree. If <paramref name="maxDepth"/> is 0, the entire tree will be searched.</param>
        /// <returns>The nearest ancestor of <paramref name="source"/> matching the specified type; if no such ancestor exists, null is returned.</returns>
        public static T FindParent<T>(this DependencyObject source, int maxDepth = 0) where T : DependencyObject
        {
            if (!(source is Visual || source is Visual3D))
                throw new ArgumentException("The value of element must represent either a Visual or Visual3D object.");

            // element is T, no need to walk the tree, just return.
            if (source is T)
                return source as T;

            // Start walking the Visual Tree to search for parents of type T.
            for (DependencyObject parent = VisualTreeHelper.GetParent(source); parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                var result = parent as T;
                if (result != null)
                    return result;

                // If maxDepth == 0, walk the entire tree
                // Otherwise stop searching when maxDepth = 1
                if (maxDepth <= 0) continue;
                if (maxDepth == 1) break;

                maxDepth--;
            }

            return null;
        }

        /// <summary>
        /// Analyzes both visual and logical tree in order to find all elements of a given type that are descendants of the <paramref name="source"/> item.
        /// </summary>
        /// <typeparam name="T">The type of the queried items.</typeparam>
        /// <param name="source">The root element that marks the source of the search. If the source is already of the requested type, it will not be included in the result.</param>
        /// <returns>All descendants of <paramref name="source"/> that match the requested type.</returns>
        public static IEnumerable<T> FindChildren<T>(this DependencyObject source) where T : DependencyObject
        {
            if (source != null)
            {
                var childs = GetChildObjects(source);
                foreach (DependencyObject child in childs)
                {
                    //analyze if children match the requested type
                    if (child is T)
                    {
                        yield return (T)child;
                    }

                    //recurse tree
                    foreach (T descendant in FindChildren<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }
        
        /// <summary>
        /// This method is an alternative to WPF's <see cref="VisualTreeHelper.GetChild"/> method, which also supports content elements.
        /// <para> Do note, that for content elements, this method falls back to the logical tree of the element.</para>
        /// </summary>
        /// <param name="parent">The item to be processed.</param>
        /// <returns>The submitted item's child elements, if available.</returns>
        private static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject parent)
        {
            if (parent == null) yield break;


            if (parent is ContentElement || parent is FrameworkElement)
            {
                //use the logical tree for content / framework elements
                foreach (object obj in LogicalTreeHelper.GetChildren(parent))
                {
                    var depObj = obj as DependencyObject;
                    if (depObj != null)
                        yield return (DependencyObject) obj;
                }
            }
            else
            {
                //use the visual tree per default
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    yield return VisualTreeHelper.GetChild(parent, i);
                }
            }
        }
    }
}