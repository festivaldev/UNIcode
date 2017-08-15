using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// ReSharper disable ValueParameterNotUsed

namespace UNIcode
{
    public partial class MainWindow : Window
    {
        #region "Properties"

        public int NewHeight { set => CalculateDimension(); }
        public int NewWidth { set => CalculateDimension(); }

        #endregion

        #region "Fields"

        private bool autoSizeEnabled;
        private int columnCount;
        private int rowCount;
        private int tileSize;

        #endregion

        #region "Ctors"

        public MainWindow() {
            InitializeComponent();
        }

        #endregion

        #region "Public Functions"

        #endregion

        #region "Private Functions"

        private void CalculateDimension() {
            if (autoSizeEnabled) {
                
            }
        }

        #endregion

        #region "Event Handlers"

        private void OnAboutClick(object sender, EventArgs e) {
            
        }

        private void OnChecked(object sender, RoutedEventArgs e) {
            
        }

        private void OnFamilyChanged(object sender, SelectionChangedEventArgs e) {
            
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            
        }

        private void OnTileSizeChanged(object sender, SelectionChangedEventArgs e) {
            
        }

        private void OnTypefaceChanged(object sender, SelectionChangedEventArgs e) {
            
        }

        private void OnUnchecked(object sender, RoutedEventArgs e) {
            
        }

        #endregion
    }
}