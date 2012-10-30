using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvalonDock;
using Halibut.Docking;
using Microsoft.Win32;

namespace Halibut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            var startPage = new StartPage();
            startPage.ShowAsDocument(dockingManager);
            Closing += OnClosing;
            Instance = this;
        }

        public void OpenFile(string path)
        {
            if (path == null || DataUtility.IsPlaintext(path))
            {
                var editor = new FileEditor(path);
                editor.ShowAsDocument(dockingManager);
            }
            else
            {
                // TODO: Raw data editor
            }
        }

        #region Event Handlers

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            var dirtiedWindows = new List<IDirtiedWindow>();
            foreach (IDirtiedWindow item in dockingManager.DockableContents.Where(d => d is IDirtiedWindow))
            {
                if (item.IsDirty)
                    dirtiedWindows.Add(item);
            }
            if (dirtiedWindows.Count != 0)
            {
                var exitConfirmation = new ExitConfirmationDialog();
                exitConfirmation.DataContext = dirtiedWindows.Select(d => d.ObjectName);
                exitConfirmation.ShowDialog();
                if (exitConfirmation.ExitConfirmationResult == ExitConfirmationResult.Cancel)
                    cancelEventArgs.Cancel = true;
                else if (exitConfirmation.ExitConfirmationResult == ExitConfirmationResult.SaveAndExit)
                {
                    foreach (var item in dirtiedWindows)
                        item.Save();
                }
            }
        }

        #endregion

        #region Commands
        // TODO: Consider splitting this into another file

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Open empty file
            OpenFile(null);
        }

        private void NewProjectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new NewProjectWindow();
            if (!window.ShowDialog().Value)
                return;
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dockingManager.ActiveDocument is IDirtiedWindow;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (dockingManager.ActiveDocument as IDirtiedWindow).Save();
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dockingManager.ActiveDocument != null;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            dockingManager.ActiveDocument.Close();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = FileEditor.FileFilter;
            if (!dialog.ShowDialog().Value)
                return;
            OpenFile(dialog.FileName);
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }

    public static class Commands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit Application", "Exit", typeof(MainWindow));
        public static readonly RoutedUICommand NewProject = new RoutedUICommand("New Project", "NewProject", typeof(MainWindow));
        public static readonly RoutedUICommand OpenProject = new RoutedUICommand("Open Project", "OpenProject", typeof(MainWindow));
    }
}
