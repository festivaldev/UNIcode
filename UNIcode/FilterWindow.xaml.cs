using System;
using System.Globalization;
using System.Linq;
using System.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UNIcode
{
    public partial class FilterWindow : Window
    {
        #region Properties

        #endregion

        #region Fields

        #endregion

        #region Ctors

        public FilterWindow() {
            InitializeComponent();

            foreach (Label label in wrpExamples.Children) {
                label.Cursor = Cursors.Hand;
                label.MouseDown += OnFilterExampleMouseDown;
            }
        }

        #endregion

        #region Public Functions

        public void Show(MainWindow owner) {
            this.Owner = owner;

            cbxBlocks.ItemsSource = UnicodeInfo.GetBlocks().Select(b => $"{b.Name} [{b.CodePointRange.ToString().Replace("..", "-")}]");
            cbxCategories.ItemsSource = Enum.GetNames(typeof(UnicodeCategory));

            cbxBlocks.SelectedIndex = cbxCategories.SelectedIndex = 0;
            chxBlockApplies.IsChecked = chxCategoryApplies.IsChecked = false;

            Show();
        }

        #endregion

        #region Private Functions

        #endregion

        #region Event Handlers

        private void OnApplyClick(object sender, EventArgs e) {
            ((MainWindow) Owner).ApplyFilter(tbxFilter.Text, chxBlockApplies.IsChecked.Value ? cbxBlocks.SelectedItem.ToString().Split('[')[0].Trim() : string.Empty, (UnicodeCategory) Enum.Parse(typeof(UnicodeCategory), cbxCategories.SelectedItem.ToString()), chxCategoryApplies.IsChecked.Value);
            Close();
        }

        private void OnBlocksSelectionChanged(object sender, SelectionChangedEventArgs e) {
            chxBlockApplies.IsChecked = true;
        }

        private void OnCategoriesSelectionChanged(object sender, SelectionChangedEventArgs e) {
            chxCategoryApplies.IsChecked = true;
        }

        private void OnFilterExampleMouseDown(object sender, MouseEventArgs e) {
            var el = (Label) sender;
            tbxFilter.Text = el.Content.ToString();
        }

        private void OnResetClick(object sender, EventArgs e) {
            ((MainWindow) Owner).ResetFilter();
            Close();
        }

        #endregion
    }
}
