using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace InventoryManager.App.Helpers
{
    public static class SearchableComboBoxHelper
    {
        public static readonly DependencyProperty IsSearchableProperty =
            DependencyProperty.RegisterAttached(
                "IsSearchable",
                typeof(bool),
                typeof(SearchableComboBoxHelper),
                new PropertyMetadata(false, OnIsSearchableChanged));

        public static bool GetIsSearchable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSearchableProperty);
        }

        public static void SetIsSearchable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSearchableProperty, value);
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(SearchableComboBoxHelper),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderProperty);
        }

        public static void SetPlaceholder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }

        private static void OnIsSearchableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                comboBox.Loaded -= ComboBox_Loaded;
                comboBox.SelectionChanged -= ComboBox_SelectionChanged;
                comboBox.DropDownClosed -= ComboBox_DropDownClosed;

                if ((bool)e.NewValue)
                {
                    comboBox.IsEditable = true;
                    comboBox.IsTextSearchEnabled = true;

                    comboBox.Loaded += ComboBox_Loaded;
                    comboBox.SelectionChanged += ComboBox_SelectionChanged;
                    comboBox.DropDownClosed += ComboBox_DropDownClosed;

                    if (comboBox.IsLoaded)
                    {
                        AttachToEditableTextBox(comboBox);
                    }
                }
            }
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox && comboBox.IsLoaded)
            {
                UpdatePlaceholder(comboBox);
            }
        }

        private static void ComboBox_Loaded(object? sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.ApplyTemplate();
                AttachToEditableTextBox(comboBox);
                UpdatePlaceholder(comboBox);
            }
        }

        private static void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                ResetFilter(comboBox);
                UpdatePlaceholder(comboBox);
            }
        }

        private static void ComboBox_DropDownClosed(object? sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                ResetFilter(comboBox);
                UpdatePlaceholder(comboBox);
            }
        }

        private static void AttachToEditableTextBox(ComboBox comboBox)
        {
            var textBox = FindChild<TextBox>(comboBox, "PART_EditableTextBox");
            if (textBox != null)
            {
                textBox.TextChanged -= EditableTextBox_TextChanged;
                textBox.GotFocus -= EditableTextBox_GotFocus;
                textBox.LostFocus -= EditableTextBox_LostFocus;

                textBox.TextChanged += EditableTextBox_TextChanged;
                textBox.GotFocus += EditableTextBox_GotFocus;
                textBox.LostFocus += EditableTextBox_LostFocus;
            }
        }

        private static void EditableTextBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var comboBox = FindParent<ComboBox>(textBox);
                if (comboBox != null)
                {
                    UpdatePlaceholder(comboBox);
                }
            }
        }

        private static void EditableTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var comboBox = FindParent<ComboBox>(textBox);
                if (comboBox != null)
                {
                    ResetFilter(comboBox);
                    UpdatePlaceholder(comboBox);
                }
            }
        }

        private static void EditableTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.IsFocused)
            {
                var comboBox = FindParent<ComboBox>(textBox);
                if (comboBox != null && comboBox.ItemsSource != null)
                {
                    string searchText = textBox.Text ?? string.Empty;
                    var view = CollectionViewSource.GetDefaultView(comboBox.ItemsSource);
                    if (view != null)
                    {
                        string searchProp = TextSearch.GetTextPath(comboBox);
                        if (string.IsNullOrEmpty(searchProp))
                            searchProp = comboBox.DisplayMemberPath;

                        if (string.IsNullOrWhiteSpace(searchText))
                        {
                            view.Filter = null;
                        }
                        else
                        {
                            view.Filter = item =>
                            {
                                string itemText = GetItemText(item, searchProp);
                                return itemText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                            };
                        }

                        if (!comboBox.IsDropDownOpen)
                        {
                            comboBox.IsDropDownOpen = true;
                        }
                    }
                    UpdatePlaceholder(comboBox);
                }
            }
        }

        private static void ResetFilter(ComboBox comboBox)
        {
            if (comboBox.ItemsSource != null)
            {
                var view = CollectionViewSource.GetDefaultView(comboBox.ItemsSource);
                if (view != null)
                {
                    view.Filter = null;
                }
            }
        }

        private static string GetItemText(object? item, string textPath)
        {
            if (item == null) return string.Empty;
            if (string.IsNullOrEmpty(textPath)) return item.ToString() ?? string.Empty;

            var prop = item.GetType().GetProperty(textPath);
            if (prop != null)
            {
                var val = prop.GetValue(item);
                return val?.ToString() ?? string.Empty;
            }
            return item.ToString() ?? string.Empty;
        }

        private static void UpdatePlaceholder(ComboBox comboBox)
        {
            var textBox = FindChild<TextBox>(comboBox, "PART_EditableTextBox");
            if (textBox == null) return;

            string placeholder = GetPlaceholder(comboBox);
            if (string.IsNullOrEmpty(placeholder)) return;

            bool isTextEmpty = string.IsNullOrEmpty(textBox.Text) && comboBox.SelectedItem == null;
            if (isTextEmpty && !textBox.IsFocused)
            {
                var visual = new Label()
                {
                    Content = placeholder,
                    Padding = new Thickness(4, 2, 0, 0),
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var brush = new VisualBrush(visual)
                {
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Center
                };

                textBox.Background = brush;
            }
            else
            {
                textBox.Background = SystemColors.WindowBrush;
            }
        }

        private static T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && (string.IsNullOrEmpty(childName) || (child is FrameworkElement fe && fe.Name == childName)))
                {
                    return typedChild;
                }

                var childOfChild = FindChild<T>(child, childName);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }
    }
}
