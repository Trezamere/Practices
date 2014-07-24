using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Practices.Mvvm")]
namespace Practices.Mvvm
{
	/// <summary> 
	/// The shared resource dictionary is a specialized resource dictionary
	/// that loads it content only once. If a second instance with the same source
	/// is created, it only merges the resources from the cache.
	/// </summary>
	/// <remarks>
    /// Taken from http://wpftutorial.net/MergedDictionaryPerformance.html
	/// </remarks>
	public sealed class SharedResourceDictionary : ResourceDictionary
	{
        /// <summary>
        /// Initializes static members of the <see cref="SharedResourceDictionary"/> class.
        /// </summary>
        static SharedResourceDictionary()
        {
            IsInDesignerMode = (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
        }

        /// <summary>
        /// Internal cache of loaded dictionaries.
        /// </summary>
        private static readonly Dictionary<Uri, WeakReference> SharedDictionaries = new Dictionary<Uri, WeakReference>();

        /// <summary>
        /// A value indicating whether the application is in designer mode.
        /// </summary>
        private static readonly bool IsInDesignerMode;

        /// <summary>
        /// Backing field for <see cref="Source"/>.
        /// </summary>
        private Uri _source;
        /// <summary>
        /// Gets or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        public new Uri Source
        {
            get { return _source; }
            set
            {
                _source = value;

                // Always load the dictionary by default in designer mode.
                if (!SharedDictionaries.ContainsKey(value) || !SharedDictionaries[value].IsAlive || IsInDesignerMode)
                {
                    // If the dictionary is not yet loaded, load it by setting the source of the base class
                    base.Source = value;

                    // add it to the cache if we're not in designer mode
                    if (!IsInDesignerMode)
                    {
                        // If the old Dictionary went out of scope and we are using the same URI, this will remove the old WeakReference.
                        SharedDictionaries[value] = new WeakReference(this);
                    }
                }
                else
                {
                    // If the dictionary is already loaded, get it from the cache
                    MergedDictionaries.Add((ResourceDictionary) SharedDictionaries[value].Target);
                }
            }
        }
	}
}