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
using AvalonDock;

namespace Halibut.Docking
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : DockableContent
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }

        public event EventHandler<OpenErrorEventArgs> OpenError;

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView.SelectedIndex != -1)
            {
                var error = listView.SelectedItem as BuildError;
                if (OpenError != null)
                    OpenError(this, new OpenErrorEventArgs(error));
            }
        }

        public class OpenErrorEventArgs : EventArgs
        {
            public BuildError Error { get; set; }

            public OpenErrorEventArgs(BuildError error)
            {
                Error = error;
            }
        }
    }
}
