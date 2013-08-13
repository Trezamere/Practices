using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Practices.Mvvm.Annotations;

namespace Practices.Mvvm
{
    public abstract partial class ViewModelBase : INotifyPropertyChanged
    {
        #region Implementations of INotifyPropertyChanged

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="property">The property that has a new value.</param>
        [NotifyPropertyChangedInvocator]
        public virtual void NotifyPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
                memberExpression = (MemberExpression)lambda.Body;

            OnPropertyChanged(memberExpression.Member.Name);
        }

        /// <summary>
        /// Raised when a property on this ViewModel has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}