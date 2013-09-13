using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Practices.Mvvm")]
namespace Practices.Mvvm
{
	/// <summary>
	/// Taken from http://wpftutorial.net/MergedDictionaryPerformance.html
	/// The shared resource dictionary is a specialized resource dictionary
	/// that loads it content only once. If a second instance with the same source
	/// is created, it only merges the resources from the cache.
	/// </summary>
	public sealed class SharedResourceDictionary : ResourceDictionary
	{
		private static readonly Dictionary<string, WeakReference> SharedDictionaries = new Dictionary<string, WeakReference>();

		private static ResourceDictionary GetResourceDictionary(string dictionaryName)
		{
			ResourceDictionary result = null;
			if (SharedDictionaries.ContainsKey(dictionaryName))
			{
				result = (ResourceDictionary)SharedDictionaries[dictionaryName].Target;
			}
			if (result == null)
			{
				string assemblyName = System.IO.Path.GetFileNameWithoutExtension(
					Assembly.GetExecutingAssembly().ManifestModule.Name);
				result = Application.LoadComponent(new Uri(assemblyName
					+ ";component/Resources/" + dictionaryName + ".xaml",
					UriKind.Relative)) as ResourceDictionary;
				SharedDictionaries[dictionaryName] = new WeakReference(result);
			}
			return result;
		}

		#region MergedDictionaries DependencyProperty

		public static readonly DependencyProperty MergedDictionariesProperty =
			DependencyProperty.RegisterAttached("MergedDictionaries", typeof (string), typeof (SharedResourceDictionary),
				new FrameworkPropertyMetadata(null, OnMergedDictionariesChanged));

		public static string GetMergedDictionaries(DependencyObject d)
		{
			return (string) d.GetValue(MergedDictionariesProperty);
		}

		public static void SetMergedDictionaries(DependencyObject d, string value)
		{
			d.SetValue(MergedDictionariesProperty, value);
		}

		private static void OnMergedDictionariesChanged(DependencyObject d,
			DependencyPropertyChangedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.NewValue as string))
			{
				foreach (string dictionaryName in (e.NewValue as string).Split(';'))
				{
					ResourceDictionary dictionary = GetResourceDictionary(dictionaryName);
					if (dictionary != null)
					{
						if (d is FrameworkElement)
						{
							(d as FrameworkElement).Resources.MergedDictionaries.Add(dictionary);
						}
						else if (d is FrameworkContentElement)
						{
							(d as FrameworkContentElement).Resources.MergedDictionaries.Add(dictionary);
						}
					}
				}
			}
		}

		#endregion
	}
}