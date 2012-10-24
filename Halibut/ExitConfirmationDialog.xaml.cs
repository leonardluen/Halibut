using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Halibut
{
    public enum ExitConfirmationResult
    {
        Cancel,
        DiscardAndExit,
        SaveAndExit
    }

    /// <summary>
    /// Interaction logic for ExitConfirmationDialog.xaml
    /// </summary>
    public partial class ExitConfirmationDialog : Window
    {
        public ExitConfirmationResult ExitConfirmationResult { get; set; }

        public ExitConfirmationDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExitConfirmationResult = ExitConfirmationResult.Cancel;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ExitConfirmationResult = ExitConfirmationResult.DiscardAndExit;
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ExitConfirmationResult = ExitConfirmationResult.SaveAndExit;
            this.Close();
        }
    }
}
