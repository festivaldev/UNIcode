using System;
using System.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace UNIcode
{
    public partial class DetailedGlyphWindow : Window
    {
        #region Properties

        #endregion

        #region Fields

        #endregion

        #region Ctors

        public DetailedGlyphWindow() {
            InitializeComponent();
        }

        #endregion

        #region Public Functions

        public void Show(FontFamily font, char glyph, string code) {
            lblGlyph.FontFamily = font;
            lblGlyph.Content = glyph.ToString();
            this.Title = $"Details - U+{code}";

            var info = UnicodeInfo.GetCharInfo(glyph);
            lblName.Content = info.Name.Replace("WITH", "\nWITH");
            lblName.ToolTip = info.Name;
            tbxCode.Text = $"U+{code} (&#x{code};) [Alt+{Convert.ToInt32(code, 16),0:D4}]";

            if (!string.IsNullOrEmpty(info.OldName))
                PrintCharInfo("Old Name", info.OldName);

            PrintCharInfo("Category", info.Category.ToString());
            PrintCharInfo("Block", info.Block);
            PrintCharInfo("Canoncial Combining Class", info.CanonicalCombiningClass.ToString());
            PrintCharInfo("Bidirectional Class", info.BidirectionalClass.ToString());
            PrintCharInfo("Contributory Properties", info.ContributoryProperties.ToString());
            PrintCharInfo("Core Properties", info.CoreProperties.ToString());

            this.Show();
        }

        #endregion

        #region Private Functions

        private void PrintCharInfo(string key, string value) {
            PrintKey(key);
            PrintValue(value);
        }

        private void PrintKey(string key) {
            var tr = new TextRange(rtbDetails.Document.ContentEnd, rtbDetails.Document.ContentEnd) { Text = $"{key}:" };
            tr.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
        }

        private void PrintValue(string value) {
            var tr = new TextRange(rtbDetails.Document.ContentEnd, rtbDetails.Document.ContentEnd) { Text = $"{value.PadLeft(300)}\n" };
            tr.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
        }

        #endregion

        #region Event Handlers

        private void OnCopyClick(object sender, EventArgs e) {
            Clipboard.SetText(lblGlyph.Content.ToString());
            ((Button) sender).Content = "Copied!";
        }

        #endregion
    }
}
