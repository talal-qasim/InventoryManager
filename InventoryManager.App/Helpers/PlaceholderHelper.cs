using System.Windows;
using System.Windows.Controls;

namespace InventoryManager.App.Helpers
{
    public static class PlaceholderHelper
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(PlaceholderHelper),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderProperty);
        }

        public static void SetPlaceholder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded -= TextBox_Loaded;
                textBox.GotFocus -= TextBox_GotFocus;
                textBox.LostFocus -= TextBox_LostFocus;
                textBox.TextChanged -= TextBox_TextChanged;

                textBox.Loaded += TextBox_Loaded;
                textBox.GotFocus += TextBox_GotFocus;
                textBox.LostFocus += TextBox_LostFocus;
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholder((TextBox)sender);
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholder((TextBox)sender);
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholder((TextBox)sender);
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholder((TextBox)sender);
        }

        private static void UpdatePlaceholder(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text) && !textBox.IsFocused)
            {
                // To avoid modifying templates, we can just use a VisualBrush.
                // It is applied only when the textbox is empty and unfocused.
                CreateVisualBrush(textBox);
            }
            else
            {
                textBox.Background = SystemColors.WindowBrush;
            }
        }

        private static void CreateVisualBrush(TextBox textBox)
        {
            var placeholderText = GetPlaceholder(textBox);
            if (string.IsNullOrEmpty(placeholderText)) return;

            var visual = new Label()
            {
                Content = placeholderText,
                Padding = new Thickness(5, 1, 1, 1),
                Foreground = System.Windows.Media.Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            var brush = new System.Windows.Media.VisualBrush(visual)
            {
                Stretch = System.Windows.Media.Stretch.None,
                AlignmentX = System.Windows.Media.AlignmentX.Left,
                AlignmentY = System.Windows.Media.AlignmentY.Center
            };

            textBox.Background = brush;
        }
    }
}
