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
using AvalonDock;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Halibut.Docking
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : DockableContent
    {
        public static bool DisableUpdates = false;

        public event EventHandler<OpenFileEventArgs> OpenFile;

        public string RootDirectory { get; set; }

        public FileBrowser(string root)
        {
            InitializeComponent();
            RootDirectory = root;
            RepopulateContents();
            var fsWatcher = new FileSystemWatcher(RootDirectory);
            fsWatcher.IncludeSubdirectories = true;
            fsWatcher.NotifyFilter = NotifyFilters.FileName |
                NotifyFilters.DirectoryName | NotifyFilters.CreationTime;
            fsWatcher.Renamed += (s, e) => RepopulateContents();
            fsWatcher.Created += (s, e) => RepopulateContents();
            fsWatcher.Deleted += (s, e) => RepopulateContents();
            fsWatcher.EnableRaisingEvents = true;
        }

        public void RepopulateContents()
        {
            if (DisableUpdates)
                return;
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    var rootItem = RepopulateContents(RootDirectory);
                    rootItem.IsExpanded = true;
                    fileTree.Items.Clear();
                    fileTree.Items.Add(rootItem);
                }));
        }

        public TreeViewItem RepopulateContents(string directory)
        {
            // TODO: Load on demand
            // TODO: .gitignore?
            var directories = Directory.GetDirectories(directory).Where(d => !Path.GetFileName(d).StartsWith("."));
            var files = Directory.GetFiles(directory).Where(f => !Path.GetFileName(f).StartsWith("."));
            var node = GenerateFolderNode(directory);
            if (directories.Count() != 0)
            {
                foreach (var dir in directories)
                    node.Items.Add(RepopulateContents(dir));
            }
            foreach (var file in files)
                node.Items.Add(GenerateFileNode(file));
            return node;
        }

        private object GenerateFileNode(string file)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            if (HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(file)) != null)
            {
                panel.Children.Add(new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Images/code-file.png")) // TODO: Grab associated icon and use it instead
                });
            }
            else
            {
                panel.Children.Add(new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Images/file.png")) // TODO: Grab associated icon and use it instead
                });
            }
            panel.Children.Add(new Label()
            {
                Content = Path.GetFileName(file)
            });
            TreeViewItem item = new TreeViewItem();
            item.Header = panel;
            item.Tag = file;
            item.MouseDoubleClick += file_MouseDoubleClick;
            item.ContextMenu = (ContextMenu)Resources["fileContextMenu"];
            return item;
        }

        void file_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;
            string file = item.Tag as string;
            var args = new OpenFileEventArgs
            {
                File = file
            };
            if (OpenFile != null)
                OpenFile(this, args);
        }

        private TreeViewItem GenerateFolderNode(string name)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/folder.png"))
            });
            panel.Children.Add(new Label()
            {
                Content = Path.GetFileName(name) // technically is a directory name, but usually works anyway
            });
            TreeViewItem item = new TreeViewItem();
            item.Header = panel;
            return item;
        }

        public class OpenFileEventArgs : EventArgs
        {
            public string File { get; set; }
        }

        private void contextMenuOpenClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
