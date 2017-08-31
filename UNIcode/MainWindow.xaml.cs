using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ValueParameterNotUsed

namespace UNIcode
{
    public partial class MainWindow : Window
    {
        #region Properties

        public ObservableCollection<string> FontFamilies { get; private set; } = new ObservableCollection<string>();
        public static bool IgnoreConfig;
        public int NewHeight { set => CalculateDimension(); }
        public int NewWidth { set => CalculateDimension(); }

        #endregion

        #region Fields

        private readonly SolidColorBrush blackBrush = new SolidColorBrush(Color.FromRgb(17, 17, 17));
        private readonly SolidColorBrush gainsboroBrush = new SolidColorBrush(Colors.Gainsboro);
        private readonly SolidColorBrush hotTrackBrush = new SolidColorBrush(SystemColors.HotTrackColor);
        private readonly SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
        private readonly XmlLanguage xmlLang = XmlLanguage.GetLanguage("en-us");

        private SolidColorBrush accentBrush;
        private SolidColorBrush backgroundBrush;
        private SolidColorBrush foregroundBrush;
        private SolidColorBrush foregroundHoverBrush;

        private string hoverBackground = "#0066CC";
        private string hoverForeground = "#FFFFFF";

        private bool autoSizeEnabled;
        private List<int> characters = new List<int>();
        private double columnCount = 10D;
        private int currentStartIndex;
        private List<int> filteredCharacters = new List<int>();
        private readonly ContextMenu mnuContext = new ContextMenu();
        private double rowCount = 5D;
        private char selectedCharacter;
        private FontFamily selectedFont;
        private int _tileSize;
        private int tileSize = 70;

        #endregion

        #region Ctors

        public MainWindow() {
            InitializeComponent();

            LoadAllFontFamilies();
            cbxFamilies.ItemsSource = FontFamilies;
            cbxTileSize.ItemsSource = Helper.GetRangeInSteps(30, 400, 10);

            accentBrush = hotTrackBrush;
            backgroundBrush = gainsboroBrush;
            foregroundBrush = blackBrush;
            foregroundHoverBrush = whiteBrush;

            var mniCopyCharacter = new MenuItem { Header = "Copy Character" };
            mniCopyCharacter.Click += (sender, e) => { Clipboard.SetText(selectedCharacter.ToString()); };
            mnuContext.Items.Add(mniCopyCharacter);

            var mniCopyUnicode = new MenuItem { Header = "Copy Unicode" };
            mniCopyUnicode.Click += (sender, e) => { Clipboard.SetText($"U+{(int) selectedCharacter,0:X4}"); };
            mnuContext.Items.Add(mniCopyUnicode);

            var mniCopyHexcode = new MenuItem { Header = "Copy Hexcode" };
            mniCopyHexcode.Click += (sender, e) => { Clipboard.SetText($"&#x{(int) selectedCharacter,0:X4};"); };
            mnuContext.Items.Add(mniCopyHexcode);

            this.Title = $"UNIcode - Version {Assembly.GetEntryAssembly().GetName().Version} Release Candidate";
        }

        #endregion

        #region Public Functions

        public void ApplyFilter(string filter, string block, UnicodeCategory category, bool categoryApplies) {
            filteredCharacters = new List<int>(characters);

            if (filter.StartsWith(":")) {
                try {
                    filter = filter.Remove(0, 1);
                    if (filter.Contains('-')) {
                        var range = filter.Split('-').Select(s => Convert.ToInt32(s, 16)).ToArray();
                        filteredCharacters = filteredCharacters.FindAll(c => c >= range[0] && c <= range[1]);
                    } else if (filter.Contains(',')) {
                        var items = filter.Split(',').Select(s => Convert.ToInt32(s, 16));
                        filteredCharacters = filteredCharacters.Intersect(items).ToList();
                    } else {
                        filteredCharacters = filteredCharacters.FindAll(c => c == Convert.ToInt32(filter, 16));
                    }
                } catch { }
            } else {
                if (!string.IsNullOrEmpty(filter)) {
                    filteredCharacters = filteredCharacters.FindAll(c => UnicodeInfo.GetName(c)?.Contains(filter.ToUpper()) ?? false);
                }
                if (!string.IsNullOrEmpty(block)) {
                    filteredCharacters = filteredCharacters.FindAll(c => UnicodeInfo.GetBlockName(c) == block);
                }
                if (categoryApplies) {
                    filteredCharacters = filteredCharacters.FindAll(c => UnicodeInfo.GetCharInfo(c).Category == category);
                }
            }

            if (filteredCharacters.Count == characters.Count) {
                ResetFilter();
            } else if (filteredCharacters.Count == 0) {
                ResetFilter();
                MessageBox.Show("No characters matched your filter.", "UNI", MessageBoxButton.OK, MessageBoxImage.Error);
                Focus();
            } else {
                currentStartIndex = 0;
                ResetScrollbar();
                ShowGlyphs();
            }
        }

        public void ResetFilter() {
            filteredCharacters.Clear();
            currentStartIndex = 0;
            ResetScrollbar();
            ShowGlyphs();
        }

        #endregion

        #region Private Functions

        private void AdjustDimension() {
            wrpGlyphs.Width = wrpGlyphs.MaxWidth = tileSize * columnCount;
            wrpGlyphs.Height = wrpGlyphs.MaxHeight = tileSize * rowCount;

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

                    if (newColumnCount == columnCount && newRowCount == rowCount && _tileSize == tileSize)
                        return;

                    _tileSize = tileSize;
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
            var left = (grdGrid.ActualWidth - wrpGlyphs.ActualWidth) / 2;
            Canvas.SetLeft(wrpGlyphs, left);

            var top = (grdGrid.ActualHeight - wrpGlyphs.ActualHeight) / 2;
            Canvas.SetTop(wrpGlyphs, top);
        }

        private void CreateGlyphTable() {
            foreach (Label label in wrpGlyphs.Children) {
                label.MouseDown -= OnLabelMouseDown;
                label.MouseEnter -= OnLabelMouseEnter;
                label.MouseLeave -= OnLabelMouseLeave;
            }

            wrpGlyphs.Children.Clear();

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
                    Background = backgroundBrush, // gainsboroBrush
                    Foreground = foregroundBrush, // blackBrush
                    Content = string.Empty,
                    Cursor = Cursors.Hand
                };

                label.MouseDown += OnLabelMouseDown;
                label.MouseEnter += OnLabelMouseEnter;
                label.MouseLeave += OnLabelMouseLeave;

                wrpGlyphs.Children.Add(label);
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

        private void LoadValuesFromConfig(UnicodeConfig config) {
            if (IgnoreConfig = config.Ignore) {
                cbxTileSize.SelectedIndex = 4;
                CalculateDimension();
                return;
            }

            chxAuto.IsChecked = false;
            autoSizeEnabled = false;

            columnCount = config.ColumnCount;
            hoverBackground = config.HoverBackground;
            accentBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(hoverBackground));
            hoverForeground = config.HoverForeground;
            foregroundHoverBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(hoverForeground));
            rowCount = config.RowCount;

            if (!string.IsNullOrEmpty(config.SelectedFamily)) {
                try {
                    cbxFamilies.SelectedItem = config.SelectedFamily;
                } catch { }
            }

            if (!string.IsNullOrEmpty(config.SelectedTypeface)) {
                try {
                    cbxTypefaces.SelectedItem = config.SelectedTypeface;
                } catch { }
            }

            try {
                cbxTileSize.SelectedItem = config.TileSize;
            } catch { }

            tbxDimension.Text = $"{columnCount}x{rowCount}";

            Height = config.WindowHeight;
            Width = config.WindowWidth;
            Left = (SystemParameters.WorkArea.Width - Width) / 2 + SystemParameters.WorkArea.Left;
            Top = (SystemParameters.WorkArea.Height - Height) / 2 + SystemParameters.WorkArea.Top;
            chxAuto.IsChecked = true;
            autoSizeEnabled = true;
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
            var count = wrpGlyphs.Children.Count;
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
                if (element.i >= wrpGlyphs.Children.Count)
                    break;

                var label = (Label) wrpGlyphs.Children[element.i];
                label.Content = element.content;
                label.ToolTip = element.toolTip;
            }
        }

        #endregion

        #region Event Handlers

        private void OnAboutClick(object sender, EventArgs e) {
            var window = new AboutWindow();
            window.ShowDialog();
        }

        private void OnChecked(object sender, RoutedEventArgs e) {
            autoSizeEnabled = true;
            tbxDimension.IsEnabled = false;
            CalculateDimension();
        }

        private void OnClosing(object sender, CancelEventArgs e) {
            var config = new UnicodeConfig {
                ColumnCount = columnCount,
                HoverBackground = hoverBackground,
                HoverForeground = hoverForeground,
                Ignore = IgnoreConfig,
                RowCount = rowCount,
                SelectedFamily = cbxFamilies.SelectedItem.ToString(),
                SelectedTypeface = cbxTypefaces.SelectedItem.ToString(),
                TileSize = tileSize,
                WindowHeight = ActualHeight,
                WindowWidth = ActualWidth
            };

            var configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UNIcode", "config.json");
            File.WriteAllText(configFile, JsonConvert.SerializeObject(config, Formatting.Indented));

            Application.Current.Shutdown(0);
        }

        private void OnFamilyChanged(object sender, SelectionChangedEventArgs e) {
            selectedFont = new FontFamily(cbxFamilies.SelectedItem.ToString());
            cbxTypefaces.ItemsSource = selectedFont.GetTypefaces().ToList().FindAll(t => !t.IsBoldSimulated && !t.IsObliqueSimulated).Select(t => t.FaceNames[xmlLang]);
            cbxTypefaces.SelectedIndex = 0;

            LoadFontGlyphs();
            OnTypefaceChanged(cbxTypefaces, new SelectionChangedEventArgs(e.RoutedEvent, new List<object>(), new List<object>()));
        }

        private void OnFormKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                var window = new FilterWindow();
                window.Show(this);
            } else if (e.Key == Key.C && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                Clipboard.SetText(selectedCharacter.ToString());
            } else if (e.Key == Key.U && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                Clipboard.SetText($"U+{(int) selectedCharacter,0:X4}");
            } else if (e.Key == Key.H && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                Clipboard.SetText($"&#x{(int) selectedCharacter,0:X4};");
            }

            if (e.Key == Key.R && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift)) {
                var configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UNIcode", "config.json");
                File.WriteAllText(configFile, JsonConvert.SerializeObject(new UnicodeConfig(), Formatting.Indented));

                currentStartIndex = 0;
                cbxFamilies.SelectedIndex = 0;
                cbxTypefaces.SelectedIndex = 0;
                LoadValuesFromConfig(new UnicodeConfig());
                AdjustDimension();
                MessageBox.Show("Successfully restored original settings.", "UNIcode", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnInputKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                try {
                    var dimensions = tbxDimension.Text.ToLower().Split('x').Select(int.Parse).ToList();
                    if (columnCount != dimensions[0] || rowCount != dimensions[1]) {
                        columnCount = dimensions[0];
                        rowCount = dimensions[1];
                        AdjustDimension();
                    }
                } catch { }
            }
        }

        private void OnLabelMouseDown(object sender, MouseButtonEventArgs e) {
            try {
                var el = (Label) sender;
                if (e.ChangedButton == MouseButton.Left && Keyboard.IsKeyUp(Key.LeftShift)) {
                    var code = $"{(int) el.Content.ToString()[0],0:X4}";

                    var window = new DetailedGlyphWindow();
                    window.Show(selectedFont, el.Content.ToString()[0], code);

                } else if (e.ChangedButton == MouseButton.Left && Keyboard.IsKeyDown(Key.LeftShift)) {
                    // TODO
                } else if (e.ChangedButton == MouseButton.Right && !string.IsNullOrEmpty(el.Content.ToString())) {
                    el.ContextMenu = mnuContext;
                    mnuContext.PlacementTarget = el;
                    mnuContext.Placement = PlacementMode.Center;
                    mnuContext.IsOpen = true;
                }
            } catch { }
        }

        private void OnLabelMouseEnter(object sender, MouseEventArgs e) {
            var el = (Label) sender;
            el.Background = accentBrush; // hotTrackBrush
            el.Foreground = foregroundHoverBrush; // whiteBrush

            if (!string.IsNullOrEmpty(el.Content.ToString()))
                selectedCharacter = el.Content.ToString()[0];
        }

        private void OnLabelMouseLeave(object sender, MouseEventArgs e) {
            var el = (Label) sender;
            el.Background = backgroundBrush; // gainsboroBrush
            el.Foreground = foregroundBrush; // blackBrush
        }

        private void OnLoaded(object sender, EventArgs e) {
            cbxFamilies.SelectedIndex = 0;
            cbxTypefaces.SelectedIndex = 0;

            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UNIcode");
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            var configFile = Path.Combine(configPath, "config.json");
            if (!File.Exists(configFile)) {
                File.WriteAllText(configFile, JsonConvert.SerializeObject(new UnicodeConfig(), Formatting.Indented));

                cbxTileSize.SelectedIndex = 4;
                CalculateDimension();
            } else {
                var config = JsonConvert.DeserializeObject<UnicodeConfig>(File.ReadAllText(configFile));
                LoadValuesFromConfig(config);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            scbVertical.Value += e.Delta < 0 ? 1 : -1;
            OnScroll(scbVertical, new ScrollEventArgs(ScrollEventType.SmallIncrement, scbVertical.Value));
        }

        private void OnScroll(object sender, ScrollEventArgs e) {
            try {
                currentStartIndex = (int) e.NewValue * (int) columnCount;
                ShowGlyphs();
            } catch { }
        }

        private void OnTileSizeChanged(object sender, SelectionChangedEventArgs e) {
            _tileSize = tileSize;
            tileSize = (int) cbxTileSize.SelectedItem;
            if (autoSizeEnabled) {
                CalculateDimension();
            } else {
                AdjustDimension();
            }
        }

        private void OnTypefaceChanged(object sender, SelectionChangedEventArgs e) {
            selectedFont = new FontFamily($"{cbxFamilies.SelectedItem} {cbxTypefaces.SelectedItem}");

            foreach (Label label in wrpGlyphs.Children) {
                label.FontFamily = selectedFont;
            }

            ShowGlyphs();
        }

        private void OnUnchecked(object sender, RoutedEventArgs e) {
            autoSizeEnabled = false;
            tbxDimension.IsEnabled = true;
        }

        #endregion
    }
}