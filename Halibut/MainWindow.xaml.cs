﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
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
        public static string UserPath { get; private set; }
        public Project CurrentProject { get; private set; }

        private StartPage StartPage { get; set; }
        private OutputWindow OutputWindow { get; set; }

        public MainWindow()
        {
            InitializeEnviornment();
            InitializeComponent();
            StartPage = new StartPage();
            StartPage.ShowAsDocument(dockingManager);
            OutputWindow = new OutputWindow();
            Closing += OnClosing;
            Instance = this;
        }

        private void InitializeEnviornment()
        {
            UserPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            UserPath = Path.Combine(UserPath, "Halibut");
            Directory.CreateDirectory(UserPath);
            Directory.CreateDirectory(Path.Combine(UserPath, "Projects"));
            Directory.CreateDirectory(Path.Combine(UserPath, "Templates"));
            Directory.CreateDirectory(Path.Combine(UserPath, "Settings"));
            CurrentProject = null;
        }

        public void OpenFile(string path)
        {
            if (path == null || DataUtility.IsPlaintext(path))
            {
                if (path != null)
                {
                    foreach (FileEditor item in dockingManager.DockableContents.Where(d => d is FileEditor))
                    {
                        if (item.FileName == path)
                        {
                            item.Focus();
                            return;
                        }
                    }
                }
                var editor = new FileEditor(path);
                editor.ShowAsDocument(dockingManager);
            }
            else
            {
                // TODO: Raw data editor
            }
        }

        /// <summary>
        /// Opens a project and adjusts the enviornment to use it.
        /// </summary>
        public void IntegrateProject(Project project)
        {
            var browser = new FileBrowser(project.RootDirectory);
            browser.Show(dockingManager, AnchorStyle.Right);
            browser.OpenFile += (s, e) => OpenFile(e.File);
            CurrentProject = project;
            StartPage.Close();
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
            var template = window.SelectedTemplate;
            var name = window.ProjectName;
            var project = template.Create(window.ProjectName, Path.Combine(window.ProjectDirectory, name));
            // Open files
            foreach (var file in template.TemplateFiles.Where(f => f.Open).OrderByDescending(f => f.Focused))
                OpenFile(Path.Combine(project.RootDirectory, template.DoReplacements(window.ProjectName, file.FileName)));
            IntegrateProject(project);
        }

        private void OpenProjectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Project Configuration Files (*.config)|*.config|All Files (*.*)|*.*";
            dialog.InitialDirectory = Path.Combine(UserPath, "Projects");
            if (dialog.ShowDialog().Value)
            {
                var project = Project.FromFile(dialog.FileName);
                // TODO: Open files
                IntegrateProject(project);
            }
        }

        private void BuildProjectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!CurrentProject.ContainsKey("build"))
            {
                MessageBox.Show("No build action specified in project configuration.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OpenFile(CurrentProject.File);
                return;
            }
            OutputWindow.Show(dockingManager, AnchorStyle.Bottom);
            OutputWindow.RunCommand(CurrentProject["build"], CurrentProject.RootDirectory, success =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                        {
                            // TODO
                        }));
                });
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

        private void CommandRequireOpenProject(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentProject != null;
        }

        #endregion
    }

    public static class Commands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit Application", "Exit", typeof(MainWindow));
        public static readonly RoutedUICommand NewProject = new RoutedUICommand("New Project", "NewProject", typeof(MainWindow));
        public static readonly RoutedUICommand OpenProject = new RoutedUICommand("Open Project", "OpenProject", typeof(MainWindow));
        public static readonly RoutedUICommand BuildProject = new RoutedUICommand("Build Project", "BuildProject", typeof(MainWindow));
    }
}
