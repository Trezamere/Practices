using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.ServiceLocation;

namespace Practices.Mvvm.IoC
{
    /// <summary>
    /// Class facilitating cooperation between Inversion of Control and MVVM design paradigms.
    /// </summary>
    public class IoC
    {
        #region Dependency and Attached Properties

        #region ViewType
        /// <summary>
        /// Attached property supporting view-first design. 
        /// Allows a view to be instantiated by the WPF engine with its dependencies resolved by a container.
        /// </summary>
        public static readonly DependencyProperty ViewTypeProperty = DependencyProperty.RegisterAttached(
            "ViewType",
            typeof (Type),
            typeof (IoC),
            new PropertyMetadata(default(Type), OnViewTypeChanged));

        public static void SetViewType(ContentPresenter target, Type value)
        {
            target.SetValue(ViewTypeProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(ContentPresenter))]
        public static Type GetViewType(ContentPresenter target)
        {
            return (Type) target.GetValue(ViewTypeProperty);
        }

        private static void OnViewTypeChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(source))
                throw new NotImplementedException("DesignMode support is not implemented.");

            var cp = (ContentPresenter)source;
            var type = GetViewType(cp);
            var view = ServiceLocator.Current.GetInstance(type);
            cp.Content = view;
        }
        #endregion

        #endregion
    }
}