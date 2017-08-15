using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace UNIcode
{
    public partial class AboutWindow : Window
    {
        #region Properties

        #endregion

        #region Fields

        #endregion

        #region Ctors

        public AboutWindow() {
            InitializeComponent();
        }

        #endregion

        #region Public Functions

        #endregion

        #region Private Functions

        #endregion

        #region Event Handlers
        
        private void OnTwitterMouseDown(object sender, MouseEventArgs e) {
            Process.Start("https://twitter.com/vainamov");
        }

        private void OnLibraryMouseDown(object sender, MouseEventArgs e) {
            Process.Start("https://github.com/GoldenCrystal/NetUnicodeInfo/blob/master/LICENSE.txt");
        }

        #endregion
    }
}
