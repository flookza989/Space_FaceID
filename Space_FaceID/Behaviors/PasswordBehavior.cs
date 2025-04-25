using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Space_FaceID.Behaviors
{
    public class PasswordBehavior : Behavior<PasswordBox>
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(PasswordBehavior),
                new PropertyMetadata(string.Empty, OnPasswordChanged));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        private bool _isChanging;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PasswordChanged += OnPasswordBoxPasswordChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PasswordChanged -= OnPasswordBoxPasswordChanged;
            base.OnDetaching();
        }

        private void OnPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            _isChanging = true;
            Password = AssociatedObject.Password;
            _isChanging = false;
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBehavior behavior && !behavior._isChanging)
            {
                behavior.AssociatedObject.Password = (string)e.NewValue;
            }
        }
    }
}
