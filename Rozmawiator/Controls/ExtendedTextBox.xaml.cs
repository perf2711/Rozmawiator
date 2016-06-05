using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;

namespace Rozmawiator.Controls
{
    /// <summary>
    /// Interaction logic for ExtendedTextBox.xaml
    /// </summary>
    public partial class ExtendedTextBox : UserControl
    {
        #region Text box exposed properties

        static ExtendedTextBox()
        {
            BackgroundProperty.OverrideMetadata(typeof(ExtendedTextBox), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), OnBackgroundPropertyChanged));
            BorderBrushProperty.OverrideMetadata(typeof(ExtendedTextBox), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0xAB, 0xAD, 0xB3)), OnBorderBrushPropertyChanged));
            ForegroundProperty.OverrideMetadata(typeof(ExtendedTextBox), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black), OnForegroundPropertyChanged));
            OpacityMaskProperty.OverrideMetadata(typeof(ExtendedTextBox), new FrameworkPropertyMetadata(null, OnOpacityMaskPropertyChanged));
        }

        #region Brushes

        private static void OnBackgroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.MainTextBox.Background = obj.Background;
        }

        private static void OnBorderBrushPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.MainTextBox.BorderBrush = obj.BorderBrush;
        }

        private static void OnForegroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.MainTextBox.Foreground = obj.Foreground;
        }

        private static void OnOpacityMaskPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.MainTextBox.OpacityMask = obj.OpacityMask;
        }

        public Brush CaretBrush
        {
            get { return (Brush)MainTextBox.GetValue(TextBoxBase.CaretBrushProperty); }
            set { MainTextBox.SetValue(TextBoxBase.CaretBrushProperty, value); }
        }

        public Brush SelectionBrush
        {
            get { return (Brush)MainTextBox.GetValue(TextBoxBase.SelectionBrushProperty); }
            set { MainTextBox.SetValue(TextBoxBase.SelectionBrushProperty, value); }
        }

        #endregion

        #region Common

        [Category("Common")]
        public double SelectionOpacity
        {
            get { return (double)MainTextBox.GetValue(TextBoxBase.SelectionOpacityProperty); }
            set { MainTextBox.SetValue(TextBoxBase.SelectionOpacityProperty, value); }
        }

        [Category("Common")]
        public bool SpellCheckIsEnabled
        {
            get { return (bool)MainTextBox.GetValue(SpellCheck.IsEnabledProperty); }
            set { MainTextBox.SetValue(SpellCheck.IsEnabledProperty, value); }
        }

        [Category("Common")]
        public string Text
        {
            get { return (string)MainTextBox.GetValue(TextBox.TextProperty); }
            set { MainTextBox.SetValue(TextBox.TextProperty, value); }
        }

        [Category("Common")]
        public int UndoLimit
        {
            get { return (int)MainTextBox.GetValue(TextBoxBase.UndoLimitProperty); }
            set { MainTextBox.SetValue(TextBoxBase.UndoLimitProperty, value); }
        }

        [Category("Common")]
        public new object DataContext
        {
            get { return MainTextBox.GetValue(DataContextProperty); }
            set { MainTextBox.SetValue(DataContextProperty, value); }
        }

        #endregion

        #region Layout

        public static readonly DependencyProperty HorizontalTextAlignmentProperty =
            DependencyProperty.Register(
                "HorizontalTextAlignment",
                typeof(HorizontalAlignment),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(HorizontalAlignment.Left, OnHorizontalTextAlignmentPropertyChanged));

        public static readonly DependencyProperty VerticalTextAlignmentProperty =
            DependencyProperty.Register(
                "VerticalTextAlignment",
                typeof(VerticalAlignment),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(VerticalAlignment.Top, OnVerticalTextAlignmentPropertyChanged));

        public static readonly DependencyProperty TextPaddingProperty =
            DependencyProperty.Register(
                "TextPadding",
                typeof(Thickness),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(new Thickness(2), OnTextPaddingPropertyChanged));

        private static void OnHorizontalTextAlignmentPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.MainTextBox.HorizontalContentAlignment = obj.HorizontalTextAlignment;
        }

        private static void OnVerticalTextAlignmentPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.MainTextBox.VerticalContentAlignment = obj.VerticalTextAlignment;
        }

        private static void OnTextPaddingPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.MainTextBox.Padding = obj.TextPadding;
        }

        [Category("Text layout")]
        public HorizontalAlignment HorizontalTextAlignment
        {
            get { return (HorizontalAlignment) GetValue(HorizontalTextAlignmentProperty); }
            set { SetValue(HorizontalTextAlignmentProperty, value); }
        }

        [Category("Text layout")]
        public VerticalAlignment VerticalTextAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalTextAlignmentProperty); }
            set { SetValue(VerticalTextAlignmentProperty, value); }
        }

        [Category("Text layout")]
        public Thickness TextPadding
        {
            get { return (Thickness)GetValue(TextPaddingProperty); }
            set { SetValue(TextPaddingProperty, value); }
        }

        #endregion

        #endregion

        #region Text box exposed events

        public event TextChangedEventHandler TextChanged;

        #endregion

        #region Input filtering

        public static readonly DependencyProperty AllowLowercaseLettersProperty =
            DependencyProperty.Register(
                "AllowLowercaseLetters",
                typeof (bool),
                typeof (ExtendedTextBox),
                new PropertyMetadata(true));

        public static readonly DependencyProperty AllowUppercaseLettersProperty =
            DependencyProperty.Register(
                "AllowUppercaseLetters",
                typeof(bool),
                typeof(ExtendedTextBox),
                new PropertyMetadata(true));

        public static readonly DependencyProperty AllowNumbersProperty =
            DependencyProperty.Register(
                "AllowNumbers",
                typeof(bool),
                typeof(ExtendedTextBox),
                new PropertyMetadata(true));

        public static readonly DependencyProperty AllowWhitespacesProperty =
            DependencyProperty.Register(
                "AllowWhitespaces",
                typeof(bool),
                typeof(ExtendedTextBox),
                new PropertyMetadata(true));

        public static readonly DependencyProperty AllowNonwordCharactersProperty =
            DependencyProperty.Register(
                "AllowNonwordCharacters",
                typeof(bool),
                typeof(ExtendedTextBox),
                new PropertyMetadata(true));

        public static readonly DependencyProperty AllowCustomProperty =
            DependencyProperty.Register(
                "AllowCustom",
                typeof(bool),
                typeof(ExtendedTextBox),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CustomRegularExpressionProperty = 
            DependencyProperty.Register(
                "CustomRegularExpression", 
                typeof(string), 
                typeof(ExtendedTextBox), 
                new PropertyMetadata("[ąćęłńóśżźĄĆĘŁŃÓŚŻŹ]"));

        [Description("Allow input of lowercase letters (i.e. abcdef)"), Category("Text filtering")]
        public bool AllowLowercaseLetters
        {
            get { return (bool)GetValue(AllowLowercaseLettersProperty); }
            set { SetValue(AllowLowercaseLettersProperty, value); }
        }

        [Description("Allow input of uppercase letters (i.e. ABCDEF)"), Category("Text filtering")]
        public bool AllowUppercaseLetters
        {
            get { return (bool)GetValue(AllowUppercaseLettersProperty); }
            set { SetValue(AllowUppercaseLettersProperty, value); }
        }

        [Description("Allow input of numbers (i.e. 12345)"), Category("Text filtering")]
        public bool AllowNumbers
        {
            get { return (bool)GetValue(AllowNumbersProperty); }
            set { SetValue(AllowNumbersProperty, value); }
        }

        [Description("Allow input of whitespaces (i.e. spaces, tabs)"), Category("Text filtering")]
        public bool AllowWhitespaces
        {
            get { return (bool)GetValue(AllowWhitespacesProperty); }
            set { SetValue(AllowWhitespacesProperty, value); }
        }

        [Description("Allow input of nonword characters (i.e. $*@_:[])"), Category("Text filtering")]
        public bool AllowNonwordCharacters
        {
            get { return (bool)GetValue(AllowNonwordCharactersProperty); }
            set { SetValue(AllowNonwordCharactersProperty, value); }
        }

        [Description("Allow input of custom regular expression"), Category("Text filtering")]
        public bool AllowCustom
        {
            get { return (bool)GetValue(AllowCustomProperty); }
            set { SetValue(AllowCustomProperty, value); }
        }

        [Description("Regular expression for custom filtering entered and pasted text."), Category("Text filtering")]
        public string CustomRegularExpression
        {
            get { return (string) GetValue(CustomRegularExpressionProperty); }
            set { SetValue(CustomRegularExpressionProperty, value); }
        }

        private string ConstructRegexp()
        {
            var regexp = "(";
            if (AllowLowercaseLetters) regexp += "[a-z]|";
            if (AllowUppercaseLetters) regexp += "[A-Z]|";
            if (AllowNumbers) regexp += @"\d|";
            if (AllowWhitespaces) regexp += @"\s|";
            if (AllowNonwordCharacters) regexp += @"[^\w\s]|_|";
            if (AllowCustom) regexp += CustomRegularExpression;
            return regexp.TrimEnd('|') + ")+";
        }

        private void RegexPreviewTextInput(object sender, TextCompositionEventArgs args)
        {
            if (!Regex.IsMatch(args.Text, $"^{ConstructRegexp()}$")) args.Handled = true;
        }

        private void RegexPasteHandler(object sender, DataObjectPastingEventArgs args)
        {
            if (!args.DataObject.GetDataPresent(typeof (string))) return;

            var pasteText = args.DataObject.GetData(typeof (string)) as string;
            if (pasteText == null || (!Regex.IsMatch(pasteText, $"^{ConstructRegexp()}$"))) args.CancelCommand();
        }

        #endregion

        #region Description label

        #region Properties

        #region Brushes

        public static readonly DependencyProperty DescriptionBackgroundProperty =
            DependencyProperty.Register(
                "DescriptionBackground",
                typeof (Brush),
                typeof (ExtendedTextBox),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent), OnDescriptionBackgroundPropertyChanged));

        public static readonly DependencyProperty DescriptionBorderBrushProperty =
            DependencyProperty.Register(
                "DescriptionBorderBrush",
                typeof(Brush),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(null, OnDescriptionBorderBrushPropertyChanged));

        public static readonly DependencyProperty DescriptionForegroundProperty =
            DependencyProperty.Register(
                "DescriptionForeground",
                typeof(Brush),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0)), OnDescriptionForegroundPropertyChanged));

        public static readonly DependencyProperty DescriptionOpacityMaskProperty =
            DependencyProperty.Register(
                "DescriptionOpacityMask",
                typeof(Brush),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(null, OnDescriptionOpacityMaskPropertyChanged));

        private static void OnDescriptionBackgroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.Background = obj.DescriptionBackground;
        }

        private static void OnDescriptionBorderBrushPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.BorderBrush = obj.DescriptionBorderBrush;
        }

        private static void OnDescriptionForegroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.Foreground = obj.DescriptionForeground;
        }

        private static void OnDescriptionOpacityMaskPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.OpacityMask = obj.DescriptionOpacityMask;
        }

        [Category("Description label brushes")]
        public Brush DescriptionBackground
        {
            get { return (Brush)GetValue(DescriptionBackgroundProperty); }
            set { SetValue(DescriptionBackgroundProperty, value); }
        }

        [Category("Description label brushes")]
        public Brush DescriptionBorderBrush
        {
            get { return (Brush)GetValue(DescriptionBorderBrushProperty); }
            set { SetValue(DescriptionBorderBrushProperty, value); }
        }

        [Category("Description label brushes")]
        public Brush DescriptionForeground
        {
            get { return (Brush)GetValue(DescriptionForegroundProperty); }
            set { SetValue(DescriptionForegroundProperty, value); }
        }

        [Category("Description label brushes")]
        public Brush DescriptionOpacityMask
        {
            get { return (Brush)GetValue(DescriptionOpacityMaskProperty); }
            set { SetValue(DescriptionOpacityMaskProperty, value); }
        }

        #endregion

        #region Common

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                "Description",
                typeof (string),
                typeof (ExtendedTextBox),
                new FrameworkPropertyMetadata("Description", OnDescriptionPropertyChanged));

        private static void OnDescriptionPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.Content = obj.Description;
        }

        [Category("Description label text")]
        public object Description
        {
            get { return GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        #endregion

        #region Appearance

        public static readonly DependencyProperty DescriptionBorderThicknessProperty =
            DependencyProperty.Register(
                "DescriptionBorderThickness",
                typeof(Thickness),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(new Thickness(0), OnDescriptionBorderThicknessPropertyChanged));

        private static void OnDescriptionBorderThicknessPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.BorderThickness = obj.DescriptionBorderThickness;
        }

        [Category("Description label appearance")]
        public Thickness DescriptionBorderThickness
        {
            get { return (Thickness) GetValue(DescriptionBorderThicknessProperty); }
            set { SetValue(DescriptionBorderThicknessProperty, value); }
        }

        #endregion

        #region Layout

        public static readonly DependencyProperty DescriptionPaddingProperty =
            DependencyProperty.Register(
                "DescriptionPadding",
                typeof(Thickness),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(new Thickness(5, 3, 3, 3), OnDescriptionPaddingPropertyChanged));

        private static void OnDescriptionPaddingPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.Padding = obj.DescriptionPadding;
        }

        [Category("Description label layout")]
        public HorizontalAlignment DescriptionHorizontalAlignment
        {
            get { return (HorizontalAlignment) DescriptionLabel.GetValue(HorizontalAlignmentProperty); }
            set { DescriptionLabel.SetValue(HorizontalAlignmentProperty, value); }
        }

        [Category("Description label layout")]
        public VerticalAlignment DescriptionVerticalAlignment
        {
            get { return (VerticalAlignment)DescriptionLabel.GetValue(VerticalAlignmentProperty); }
            set { DescriptionLabel.SetValue(VerticalAlignmentProperty, value); }
        }

        [Category("Description label layout")]
        public Thickness DescriptionPadding
        {
            get { return (Thickness)GetValue(DescriptionPaddingProperty); }
            set { SetValue(DescriptionPaddingProperty, value); }
        }

        #endregion

        #region Font

        public static readonly DependencyProperty DescriptionFontFamilyProperty =
            DependencyProperty.Register(
                "DescriptionFontFamily",
                typeof(FontFamily),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(new FontFamily("Segoe UI"), OnDescriptionFontFamilyPropertyChanged));

        public static readonly DependencyProperty DescriptionFontSizeProperty =
            DependencyProperty.Register(
                "DescriptionFontSize",
                typeof(double),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(9.0, OnDescriptionFontSizePropertyChanged));

        public static readonly DependencyProperty DescriptionFontStyleProperty =
            DependencyProperty.Register(
                "DescriptionFontStyle",
                typeof(FontStyle),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(FontStyles.Italic, OnDescriptionFontStylePropertyChanged));

        public static readonly DependencyProperty DescriptionFontWeightProperty =
            DependencyProperty.Register(
                "DescriptionFontWeight",
                typeof(FontWeight),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(FontWeights.Light, OnDescriptionFontWeightPropertyChanged));

        public static readonly DependencyProperty DescriptionFontStretchProperty =
            DependencyProperty.Register(
                "DescriptionFontStretch",
                typeof(FontStretch),
                typeof(ExtendedTextBox),
                new FrameworkPropertyMetadata(FontStretches.Normal, OnDescriptionFontStretchPropertyChanged));

        private static void OnDescriptionFontFamilyPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.FontFamily = obj.DescriptionFontFamily;
        }

        private static void OnDescriptionFontSizePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.FontSize = obj.DescriptionFontSize;
        }

        private static void OnDescriptionFontStylePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.FontStyle = obj.DescriptionFontStyle;
        }

        private static void OnDescriptionFontWeightPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.FontWeight = obj.DescriptionFontWeight;
        }

        private static void OnDescriptionFontStretchPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as ExtendedTextBox;
            if (obj == null) return;

            obj.DescriptionLabel.FontStretch = obj.DescriptionFontStretch;
        }

        [Category("Description label text")]
        public FontFamily DescriptionFontFamily
        {
            get { return (FontFamily)GetValue(DescriptionFontFamilyProperty); }
            set { SetValue(DescriptionFontFamilyProperty, value); }
        }

        [Category("Description label text")]
        public double DescriptionFontSize
        {
            get { return (double)GetValue(DescriptionFontSizeProperty); }
            set { SetValue(DescriptionFontSizeProperty, value); }
        }

        [Category("Description label text")]
        public FontStyle DescriptionFontStyle
        {
            get { return (FontStyle)GetValue(DescriptionFontStyleProperty); }
            set { SetValue(DescriptionFontStyleProperty, value); }
        }

        [Category("Description label text")]
        public FontWeight DescriptionFontWeight
        {
            get { return (FontWeight)GetValue(DescriptionFontWeightProperty); }
            set { SetValue(DescriptionFontWeightProperty, value); }
        }

        [Category("Description label text")]
        public FontStretch DescriptionFontStretch
        {
            get { return (FontStretch)GetValue(DescriptionFontStretchProperty); }
            set { SetValue(DescriptionFontStretchProperty, value); }
        }

        #endregion

        #endregion

        #region Logic

        private void SetDescriptionVisibility()
        {
            DescriptionLabel.Visibility = Text.Length != 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #endregion

        public ExtendedTextBox()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(MainTextBox, RegexPasteHandler);
        }


        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(IsLoaded)
                SetDescriptionVisibility();

            TextChanged?.Invoke(this, e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetDescriptionVisibility();
        }
    }
}
