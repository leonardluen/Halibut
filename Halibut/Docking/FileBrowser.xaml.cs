using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : DockableContent
    {
        public string RootDirectory { get; set; }

        public FileBrowser(string root)
        {
            InitializeComponent();
        }

        public void RepopulateContents()
        {
            var directories = Directory.GetDirectories(RootDirectory, null, SearchOption.AllDirectories);
        }
    }
}
