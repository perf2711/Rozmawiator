using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rozmawiator.Server.Controls
{
    /// <summary>
    /// Interaction logic for SettingsSectionControl.xaml
    /// </summary>
    public partial class SettingsSectionControl : UserControl
    {
        private readonly Type _configSection;
        private readonly SolidColorBrush _validColor = new SolidColorBrush(Colors.Gray);
        private readonly SolidColorBrush _errorColor = new SolidColorBrush(Colors.Red);

        private IEnumerable<TextBox> FieldBoxes
            => SectionFields.Children.OfType<Grid>().SelectMany(g => g.Children.OfType<TextBox>());

        public bool IsValid
        {
            get { return FieldBoxes.All(t => !Equals(t.BorderBrush, _errorColor)); }
        }

        public string Title
        {
            get { return (string) SectionTitle.GetValue(ContentProperty); }
            set { SectionTitle.SetValue(ContentProperty, value); }
        }

        public SettingsSectionControl(Type configSection)
        {
            InitializeComponent();

            _configSection = configSection;
            AddControls();
        }

        public bool UpdateSection()
        {
            foreach (var textBox in FieldBoxes)
            {
                var property = textBox?.Tag as PropertyInfo;
                if (property == null)
                {
                    continue;
                }

                var propertyType = property.PropertyType;
                var converter = TypeDescriptor.GetConverter(propertyType);

                try
                {
                    property.SetValue(null, converter.ConvertFromInvariantString(textBox.Text));
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private void AddControls()
        {
            foreach (var property in _configSection.GetProperties())
            {
                AddControl(property);
            }
        }

        private void AddControl(PropertyInfo property)
        {
            var label = new Label
            {
                Content = property.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = property,
            };
            Grid.SetColumn(label, 0);

            var inputField = new TextBox
            {
                Text = property.GetValue(null)?.ToString() ?? "",
                Tag = property,
                BorderBrush = _validColor
            };
            //inputField.TextChanged += InputFieldOnTextChanged;
            Grid.SetColumn(inputField, 1);

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(2, GridUnitType.Star)}
                },
                Margin = new Thickness(0, 0, 0, 5)
            };

            grid.Children.Add(label);
            grid.Children.Add(inputField);

            SectionFields.Children.Add(grid);
        }

        private void InputFieldOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var textBox = sender as TextBox;
            var property = textBox?.Tag as PropertyInfo;
            if (property == null)
            {
                return;
            }

            var propertyType = property.PropertyType;
            var converter = TypeDescriptor.GetConverter(propertyType);

            try
            {
                property.SetValue(null, converter.ConvertFromInvariantString(textBox.Text));
                textBox.BorderBrush = _validColor;
            }
            catch (Exception)
            {
                textBox.BorderBrush = _errorColor;
            }
        }
    }
}
