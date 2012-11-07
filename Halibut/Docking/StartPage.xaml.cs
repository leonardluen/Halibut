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
using Halibut.Settings;

namespace Halibut.Docking
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : DockableContent
    {
        public StartPage()
        {
            InitializeComponent();
            var recent = RecentProjects.GetRecentProjects();
            if (recent.Count == 0)
                recentNone.Visibility = Visibility.Visible;
            else
            {
                recentNone.Visibility = Visibility.Collapsed;
                recentList.ItemsSource = recent;
            }
        }

        private void recentProject_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.OpenProject((sender as Button).DataContext as Project);
        }
    }
}
