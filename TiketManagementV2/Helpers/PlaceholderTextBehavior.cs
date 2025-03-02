using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace TiketManagementV2.Helpers
{
    public static class PlaceholderTextBehavior
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.RegisterAttached(
                "PlaceholderText",
                typeof(string),
                typeof(PlaceholderTextBehavior),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public static string GetPlaceholderText(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderTextProperty);
        }

        public static void SetPlaceholderText(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderTextProperty, value);
        }

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                    ShowPlaceholder(textBox, (string)e.NewValue);

                textBox.GotFocus += TextBox_GotFocus;
                textBox.LostFocus += TextBox_LostFocus;
                textBox.TextChanged += TextBox_TextChanged;
            }
            else if (d is PasswordBox passwordBox)
            {
                if (passwordBox.Password.Length == 0)
                    ShowPlaceholder(passwordBox, (string)e.NewValue);

                passwordBox.GotFocus += PasswordBox_GotFocus;
                passwordBox.LostFocus += PasswordBox_LostFocus;
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Tag as string == "Placeholder")
            {
                HidePlaceholder(textBox);
            }
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                ShowPlaceholder(textBox, GetPlaceholderText(textBox));
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Tag as string != "Placeholder" && string.IsNullOrEmpty(textBox.Text))
            {
                ShowPlaceholder(textBox, GetPlaceholderText(textBox));
            }
        }

        private static void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (passwordBox.Tag as string == "Placeholder")
            {
                HidePlaceholder(passwordBox);
            }
        }

        private static void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (passwordBox.Password.Length == 0)
            {
                ShowPlaceholder(passwordBox, GetPlaceholderText(passwordBox));
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (passwordBox.Tag as string != "Placeholder" && passwordBox.Password.Length == 0)
            {
                ShowPlaceholder(passwordBox, GetPlaceholderText(passwordBox));
            }
        }

        private static void ShowPlaceholder(TextBox textBox, string placeholderText)
        {
            textBox.Text = placeholderText;
            textBox.Foreground = new SolidColorBrush(Colors.Gray);
            textBox.Tag = "Placeholder";
        }

        private static void HidePlaceholder(TextBox textBox)
        {
            textBox.Text = string.Empty;
            textBox.Foreground = new SolidColorBrush(Colors.White);
            textBox.Tag = null;
        }

        private static void ShowPlaceholder(PasswordBox passwordBox, string placeholderText)
        {
            // Sử dụng TextBox để hiển thị placeholder
            TextBox textBox = new TextBox
            {
                Text = placeholderText,
                Foreground = new SolidColorBrush(Colors.Gray),
                Background = passwordBox.Background,
                FontSize = passwordBox.FontSize,
                FontFamily = passwordBox.FontFamily,
                FontWeight = passwordBox.FontWeight,
                Padding = passwordBox.Padding,
                Margin = new Thickness(0),
                VerticalContentAlignment = passwordBox.VerticalContentAlignment,
                HorizontalContentAlignment = passwordBox.HorizontalContentAlignment,
                BorderThickness = new Thickness(0)
            };

            Grid parent = passwordBox.Parent as Grid;
            if (parent != null)
            {
                int row = Grid.GetRow(passwordBox);
                int column = Grid.GetColumn(passwordBox);

                parent.Children.Add(textBox);
                Grid.SetRow(textBox, row);
                Grid.SetColumn(textBox, column);

                passwordBox.Visibility = Visibility.Hidden;
                textBox.Tag = passwordBox;
                textBox.GotFocus += PlaceholderTextBox_GotFocus;
            }
        }

        private static void PlaceholderTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            PasswordBox passwordBox = textBox.Tag as PasswordBox;
            if (passwordBox != null)
            {
                textBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                passwordBox.Focus();
            }
        }

        private static void HidePlaceholder(PasswordBox passwordBox)
        {
            passwordBox.Tag = null;
        }
    }
}
