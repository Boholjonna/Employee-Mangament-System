using System;
using System.Windows;
using System.Windows.Controls;

namespace SOFTDEV.Views
{
    public partial class AdminToDoTab : UserControl
    {
        /// <summary>
        /// Called when the user clicks the Back button.
        /// Set this before adding the tab to the visual tree.
        /// </summary>
        public Action? OnBack { get; set; }

        public AdminToDoTab()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            OnBack?.Invoke();
        }
    }
}
