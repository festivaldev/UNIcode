using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ValueParameterNotUsed

namespace UNIcode
{
    public partial class MainWindow : Window
    {
        #region Properties

        public ObservableCollection<string> FontFamilies { get; private set; } = new ObservableCollection<string>();
        public int NewHeight { set => CalculateDimension(); }
        public int NewWidth { set => CalculateDimension(); }

        #endregion

        #region Fields

        private readonly SolidColorBrush blackBrush = new SolidColorBrush(Color.FromRgb(17, 17, 17));
        private readonly SolidColorBrush gainsboroBrush = new SolidColorBrush(Colors.Gainsboro);
        private readonly SolidColorBrush hotTrackBrush = new SolidColorBrush(SystemColors.HotTrackColor);
        private readonly SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
        private readonly XmlLanguage xmlLang = XmlLanguage.GetLanguage("en-us");

        private bool autoSizeEnabled;
        private List<int> characters = new List<int>();
        private int currentStartIndex;
        private double columnCount = 10D;
        private List<int> filteredCharacters = new List<int>();
        private double rowCount = 5D;
        private FontFamily selectedFont;
        private int tileSize = 70;

        #endregion

        #region Ctors

        public MainWindow() {
            InitializeComponent();

            LoadAllFontFamilies();
            cbxFamilies.ItemsSource = FontFamilies;
            cbxTileSize.ItemsSource = Helper.GetRangeInSteps(30, 300, 10);
        }

        #endregion

        #region Public Functions

        #endregion

        #region Private Functions

        private void AdjustDimension() {
            wrpGlyphes.Width = wrpGlyphes.MaxWidth = tileSize * columnCount;
            wrpGlyphes.Height = wrpGlyphes.MaxHeight = tileSize * rowCount;

            CreateGlyphTable();
            ShowGlyphs();
            CenterGlyphTable();
        }

        private void CalculateDimension() {
            try {
                if (autoSizeEnabled) {
                    var width = (skpPanel?.ActualWidth ?? 744) - 17;
                    var height = (grdMain?.ActualHeight ?? 461) - 100;

                    var newColumnCount = Math.Floor(width / tileSize);
                    var newRowCount = Math.Floor(height / tileSize);

                    if (newColumnCount == columnCount && newRowCount == rowCount)
                        return;

                    columnCount = newColumnCount;
                    rowCount = newRowCount;
                    tbxDimension.Text = $"{columnCount}x{rowCount}";
                    AdjustDimension();
                } else {
                    CenterGlyphTable();
                }
            } catch { }
        }

        private void CenterGlyphTable() {
            var left = (grdGrid.ActualWidth - wrpGlyphes.ActualWidth) / 2;
            Canvas.SetLeft(wrpGlyphes, left);

            var top = (grdGrid.ActualHeight - wrpGlyphes.ActualHeight) / 2;
            Canvas.SetTop(wrpGlyphes, top);
        }

        private void CreateGlyphTable() {
            foreach (Label label in wrpGlyphes.Children) {
                label.MouseDown -= OnLabelMouseDown;
                label.MouseEnter -= OnLabelMouseEnter;
                label.MouseLeave -= OnLabelMouseLeave;
            }

            wrpGlyphes.Children.Clear();

            for (var i = 0; i < (int) columnCount * rowCount; i++) {
                var label = new Label {
                    Height = tileSize,
                    Width = tileSize,
                    FontFamily = selectedFont,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = tileSize / 2,
                    BorderThickness = new Thickness(.5),
                    BorderBrush = whiteBrush,
                    Background = gainsboroBrush,
                    Foreground = blackBrush,
                    Content = string.Empty,
                    Cursor = Cursors.Hand
                };

                label.MouseDown += OnLabelMouseDown;
                label.MouseEnter += OnLabelMouseEnter;
                label.MouseLeave += OnLabelMouseLeave;

                wrpGlyphes.Children.Add(label);
            }

            ResetScrollbar();
        }

        private void LoadAllFontFamilies() {
            FontFamilies = new ObservableCollection<string>(Fonts.SystemFontFamilies.OrderBy(f => f.FamilyNames[xmlLang]).Select(f => f.FamilyNames[xmlLang]));
        }

        private void LoadFontGlyphs() {
            foreach (var typeface in selectedFont.GetTypefaces()) {
                typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface);
                if (glyphTypeface != null) {
                    characters = glyphTypeface.CharacterToGlyphMap.Select(kv => kv.Key).ToList();
                    break;
                }
            }

            ResetScrollbar();
        }

        private void ResetScrollbar() {
            if (filteredCharacters.Count == 0) {
                scbVertical.Maximum = Math.Ceiling(characters.Count / columnCount) - rowCount;
            } else {
                scbVertical.Maximum = Math.Ceiling(filteredCharacters.Count / columnCount) - rowCount;
            }
            scbVertical.Value = 0;
            currentStartIndex = 0;
        }

        private async void ShowGlyphs() {
            var count = wrpGlyphes.Children.Count;
            var data = new List<(int i, string content, string toolTip)>();

            await Task.Run(() => {
                var n = 0;
                for (var i = currentStartIndex; i < currentStartIndex + count && n < count; i++) {
                    try {
                        string content;
                        string toolTip;

                        if (filteredCharacters.Count == 0) {
                            content = i < characters.Count ? Convert.ToChar(characters[i]).ToString() : string.Empty;
                            toolTip = i < characters.Count ? UnicodeInfo.GetName(characters[i]) : null;
                        } else {
                            content = i < filteredCharacters.Count ? Convert.ToChar(filteredCharacters[i]).ToString() : string.Empty;
                            toolTip = i < filteredCharacters.Count ? UnicodeInfo.GetName(filteredCharacters[i]) : null;
                        }

                        data.Add((n, content, toolTip));
                    } catch { }
                    n++;
                }
            });

            foreach (var element in data) {
                var label = (Label) wrpGlyphes.Children[element.i];
                label.Content = element.content;
                label.ToolTip = element.toolTip;
            }
        }

        #endregion

        #region Event Handlers

        private void OnAboutClick(object sender, EventArgs e) {
            // TODO
        }

        private void OnChecked(object sender, RoutedEventArgs e) {
            autoSizeEnabled = true;
            tbxDimension.IsEnabled = false;
            CalculateDimension();
        }

        private void OnFamilyChanged(object sender, SelectionChangedEventArgs e) {
            selectedFont = new FontFamily(cbxFamilies.SelectedItem.ToString());
            cbxTypefaces.ItemsSource = selectedFont.GetTypefaces().ToList().FindAll(t => !t.IsBoldSimulated && !t.IsObliqueSimulated).Select(t => t.FaceNames[xmlLang]);
            cbxTypefaces.SelectedIndex = 0;

            LoadFontGlyphs();
        }

        private void OnFormKeyDown(object sender, KeyEventArgs e) {
            // TODO
        }

        private void OnInputKeyDown(object sender, KeyEventArgs e) {

        }

        private void OnLabelMouseDown(object sender, MouseButtonEventArgs e) {
            try {
                var el = (Label) sender;
                if (e.ChangedButton == MouseButton.Left && Keyboard.IsKeyUp(Key.LeftShift)) {
                    // TODO
                } else if (e.ChangedButton == MouseButton.Left && Keyboard.IsKeyDown(Key.LeftShift)) {
                    // TODO
                }
            } catch { }
        }

        private void OnLabelMouseEnter(object sender, MouseEventArgs e) {
            var el = (Label) sender;
            el.Background = hotTrackBrush;
            el.Foreground = whiteBrush;
        }

        private void OnLabelMouseLeave(object sender, MouseEventArgs e) {
            var el = (Label) sender;
            el.Background = gainsboroBrush;
            el.Foreground = blackBrush;
        }

        private void OnLoaded(object sender, EventArgs e) {

        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {

        }

        private void OnScroll(object sender, ScrollEventArgs e) {

        }

        private void OnTileSizeChanged(object sender, SelectionChangedEventArgs e) {
            tileSize = (int) cbxTileSize.SelectedItem;
            if (autoSizeEnabled) {
                CalculateDimension();
            } else {
                AdjustDimension();
            }
        }

        private void OnTypefaceChanged(object sender, SelectionChangedEventArgs e) {
            selectedFont = new FontFamily($"{cbxFamilies.SelectedItem} {cbxTypefaces.SelectedItem}");
            tbxString.FontFamily = selectedFont;

            foreach (Label label in wrpGlyphes.Children) {
                label.FontFamily = selectedFont;
            }
        }

        private void OnUnchecked(object sender, RoutedEventArgs e) {
            autoSizeEnabled = false;
            tbxDimension.IsEnabled = true;
        }

        #endregion
    }
}