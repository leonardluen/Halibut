using System;
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
using System.Text.RegularExpressions;
using Halibut.Settings;
using System.Diagnostics;

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
        private ErrorWindow ErrorWindow { get; set; }

        public Process Debugger { get; set; }

        public MainWindow()
        {
            InitializeEnviornment();
            InitializeComponent();
            StartPage = new StartPage();
            StartPage.ShowAsDocument(dockingManager);
            OutputWindow = new OutputWindow();
            ErrorWindow = new ErrorWindow();
            ErrorWindow.OpenError += ErrorWindow_OpenError;
            Closing += OnClosing;
            Instance = this;
            Drop += MainWindow_Drop;
            AllowDrop = true;
        }

        void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                    OpenFile(file);
            }
        }

        private void InitializeEnviornment()
        {
            FileEditor.Initialize();
            UserPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            UserPath = Path.Combine(UserPath, "Halibut");
            Directory.CreateDirectory(UserPath);
            Directory.CreateDirectory(Path.Combine(UserPath, "Projects"));
            Directory.CreateDirectory(Path.Combine(UserPath, "Templates"));
            Directory.CreateDirectory(Path.Combine(UserPath, "Settings"));
            CurrentProject = null;
        }

        public FileEditor OpenFile(string path)
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
                            return item;
                        }
                    }
                }
                var editor = new FileEditor(path);
                editor.ShowAsDocument(dockingManager);
                return editor;
            }
            else
            {
                // TODO: Raw data editor
                return null;
            }
        }

        /// <summary>
        /// Opens a project and adjusts the enviornment to use it.
        /// </summary>
        public void OpenProject(Project project)
        {
            var browser = new FileBrowser(project.RootDirectory);
            browser.Show(dockingManager, AnchorStyle.Left);
            browser.OpenFile += (s, e) => OpenFile(e.File);
            CurrentProject = project;
            StartPage.Close();
            Title = project.Name + " - Halibut";
            RecentProjects.ProjectOpened(project);
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

        void ErrorWindow_OpenError(object sender, ErrorWindow.OpenErrorEventArgs e)
        {
            var file = Path.Combine(CurrentProject.RootDirectory, e.Error.File);
            var editor = OpenFile(file);
            if (editor == null)
                return;
            var offset = editor.textEditor.Document.GetOffset(e.Error.LineNumber, 0);
            var line = editor.textEditor.Document.GetLineByOffset(offset);
            editor.textEditor.ScrollToLine(e.Error.LineNumber);
            editor.textEditor.Select(offset, line.Length);
            editor.Focus();
            editor.textEditor.Focus();
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
            OpenProject(project);
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
                OpenProject(project);
            }
        }

        private void BuildProjectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var callback = e.Parameter as Action;
            Commands.SaveAll.Execute(null, this);
            if (!CurrentProject.ContainsKey("build"))
            {
                MessageBox.Show("No build action specified in project configuration.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OpenFile(CurrentProject.File);
                return;
            }
            OutputWindow.Show(dockingManager, AnchorStyle.Bottom);
            var workingDirectory = CurrentProject.RootDirectory;
            if (CurrentProject.ContainsKey("working-directory"))
            {
                workingDirectory = CurrentProject["working-directory"];
                if (!Path.IsPathRooted(workingDirectory))
                    workingDirectory = Path.Combine(CurrentProject.RootDirectory, workingDirectory);
            }
            OutputWindow.RunCommand(CurrentProject["build"], workingDirectory, result =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (result.ReturnCode == 0)
                                statusText.Text = "Build succeeded";
                            else
                                statusText.Text = "Build failed";
                            if (CurrentProject.ContainsKey("error-regex") && result.Output != null)
                            {
                                var errorRegex = new Regex(CurrentProject["error-regex"]);
                                var matches = errorRegex.Matches(result.Output);
                                var errors = new List<BuildError>();
                                foreach (var match in matches)
                                {
                                    var error = new BuildError();
                                    var item = errorRegex.Match(match.ToString());
                                    if (item.Groups["file"] != null)
                                        error.File = item.Groups["file"].Value.Replace("\r", "");
                                    if (item.Groups["error"] != null)
                                        error.Error = item.Groups["error"].Value.Replace("\r", "");
                                    if (item.Groups["line"] != null)
                                        error.LineNumber = int.Parse(item.Groups["line"].Value);
                                    errors.Add(error);
                                }
                                if (errors.Count != 0)
                                {
                                    ErrorWindow.DataContext = errors;
                                    ErrorWindow.Show(dockingManager, AnchorStyle.Bottom);
                                }
                            }
                            if (callback != null)
                                callback();
                        }));
                });
        }

        private void StartDebuggingCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Debugger == null && CurrentProject != null;
        }

        private void StartDebuggingCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Strongly consider refactoring debugging
            Commands.SaveAll.Execute(null, this);
            Commands.BuildProject.Execute(new Action(() => 
                {
                    var workingDirectory = CurrentProject.RootDirectory;
                    if (CurrentProject.ContainsKey("working-directory"))
                    {
                        workingDirectory = CurrentProject["working-directory"];
                        if (!Path.IsPathRooted(workingDirectory))
                            workingDirectory = Path.Combine(CurrentProject.RootDirectory, workingDirectory);
                        var command = CurrentProject["debug"];
                        var startInfo = GetStartInfo(command, workingDirectory);
                        startInfo.CreateNoWindow = true;
                        Debugger = new Process();
                        Debugger.Exited += Debugger_Exited;
                        Debugger.StartInfo = startInfo;
                        Dispatcher.BeginInvoke(new Action(() => statusText.Text = "Debugging"));
                        Debugger.Start();
                    }

                }), this);
        }

        void Debugger_Exited(object sender, EventArgs e)
        {
            Debugger = null;
            Dispatcher.BeginInvoke(new Action(() => statusText.Text = "Ready"));
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dockingManager.ActiveDocument is IDirtiedWindow;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (dockingManager.ActiveDocument as IDirtiedWindow).Save();
        }

        private void SaveAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (IDirtiedWindow item in dockingManager.DockableContents.Where(d => d is IDirtiedWindow))
                item.Save();
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

        private void NextDocumentCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (dockingManager.ActiveDocument != null)
            {
                var index = dockingManager.MainDocumentPane.Items.IndexOf(dockingManager.ActiveDocument as DockableContent);
                index++;
                if (index >= dockingManager.MainDocumentPane.Items.Count)
                    index = 0;
                (dockingManager.MainDocumentPane.Items[index] as DockableContent).Focus();
            }
        }

        #endregion

        public static ProcessStartInfo GetStartInfo(string command, string workingDirectory) // TODO: Consider moving this
        {
            var startInfo = new ProcessStartInfo();
            if (!command.Contains(' '))
                startInfo.FileName = command;
            else
                startInfo.FileName = command.Remove(command.IndexOf(' '));
            if (!File.Exists(startInfo.FileName))
            {
                if (File.Exists(Path.Combine(workingDirectory, startInfo.FileName)))
                    startInfo.FileName = Path.Combine(workingDirectory, startInfo.FileName);
                else
                {
                    // Attempt to find it in the path
                    var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
                    foreach (var item in path.Split(';'))
                    {
                        if (File.Exists(Path.Combine(item, startInfo.FileName)))
                        {
                            startInfo.FileName = Path.Combine(item, startInfo.FileName);
                            break;
                        }
                    }
                }
            }
            if (command.Contains(' '))
                startInfo.Arguments = command.Substring(command.SafeIndexOf(' ') + 1);
            startInfo.WorkingDirectory = workingDirectory;
            return startInfo;
        }
    }

    public static class Commands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit Application", "Exit", typeof(MainWindow));
        public static readonly RoutedUICommand NewProject = new RoutedUICommand("New Project", "NewProject", typeof(MainWindow));
        public static readonly RoutedUICommand OpenProject = new RoutedUICommand("Open Project", "OpenProject", typeof(MainWindow));
        public static readonly RoutedUICommand BuildProject = new RoutedUICommand("Build Project", "BuildProject", typeof(MainWindow));
        public static readonly RoutedUICommand SaveAll = new RoutedUICommand("Save All", "SaveAll", typeof(MainWindow));
        public static readonly RoutedUICommand StartDebugging = new RoutedUICommand("Start Debugging", "StartDebugging", typeof(MainWindow));
        public static readonly RoutedUICommand NextDocument = new RoutedUICommand("Next Document", "NextDocument", typeof(MainWindow));
        public static readonly RoutedUICommand PreviousDocument = new RoutedUICommand("Previous Document", "PreviousDocument", typeof(MainWindow));
    }
}
