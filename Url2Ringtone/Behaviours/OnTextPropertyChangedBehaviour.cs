using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Microsoft.Phone.Controls;
using System.Windows.Data;

namespace Url2Ringtone.Behaviours
{
    public class UpdateTextBindingOnPropertyChanged : Behavior<PhoneTextBox>
    {
        // Fields
        private BindingExpression expression;

        // Methods
        protected override void OnAttached()
        {
            base.OnAttached();
            this.expression = base.AssociatedObject.GetBindingExpression(PhoneTextBox.TextProperty);
            base.AssociatedObject.TextChanged += OnTextChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            base.AssociatedObject.TextChanged -= OnTextChanged;
            this.expression = null;
        }

        private void OnTextChanged(object sender, EventArgs args)
        {
            this.expression.UpdateSource();
        }
    }
}
