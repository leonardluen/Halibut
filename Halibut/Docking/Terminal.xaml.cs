using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AvalonDock;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

namespace Halibut.Docking
{
    /// <summary>
    /// Interaction logic for Terminal.xaml
    /// </summary>
    public partial class Terminal : DockableContent
    {
        public string WorkingDirectory { get; set; }
        private Process CurrentProcess { get; set; }
        private Timer StandardOutTimer { get; set; }
        private ObservableCollection<string> Output { get; set; }

        public Terminal()
        {
            InitializeComponent();
            StandardOutTimer = new Timer(StandardOutTick, null, Timeout.Infinite, Timeout.Infinite);
            Output = new ObservableCollection<string>();
            outputView.ItemsSource = Output;
            WorkingDirectory = Directory.GetCurrentDirectory();
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (CurrentProcess == null)
                    ExecuteCommand(textBox.Text);
                else
                {
                    try
                    {
                        CurrentProcess.StandardInput.WriteLine(textBox.Text);
                        textBox.Text = "";
                    }
                    catch {}
                }
            }
        }

        public void ExecuteCommand(string command)
        {
            textBox.Text = "";
            string parameters = "";
            if (command.Contains(' '))
            {
                parameters = command.Substring(command.IndexOf(' ') + 1);
                command = command.Remove(command.IndexOf(' '));
            }
            if (command.ToLower() == "cd")
            {
                if (string.IsNullOrEmpty(parameters))
                {
                    Write(WorkingDirectory);
                    return;
                }
                WorkingDirectory = System.IO.Path.Combine(WorkingDirectory, parameters);
                WorkingDirectory = System.IO.Path.GetFullPath(WorkingDirectory);
                return;
            }
            if (command.ToLower() == "clear" || command.ToLower() == "cls")
            {
                Output.Clear();
                return;
            }
            try
            {
                if (File.Exists(System.IO.Path.Combine(WorkingDirectory, command)))
                    command = System.IO.Path.Combine(WorkingDirectory, command);
                if (File.Exists(System.IO.Path.Combine(WorkingDirectory, command, ".exe")))
                    command = System.IO.Path.Combine(WorkingDirectory, command, ".exe");
                var info = new ProcessStartInfo(command, parameters);
                info.UseShellExecute = false;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                info.RedirectStandardInput = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.WorkingDirectory = WorkingDirectory;
                info.CreateNoWindow = true;
                CurrentProcess = new Process();
                CurrentProcess.StartInfo = info;
                CurrentProcess.EnableRaisingEvents = true;
                CurrentProcess.Exited += CurrentProcessOnExited;
                CurrentProcess.Start();
                StandardOutTimer.Change(10, 10);
                this.Focus();
            }
            catch (Exception e)
            {
                Write(e.ToString());
                CurrentProcess = null;
            }
        }

        private void CurrentProcessOnExited(object sender, EventArgs eventArgs)
        {
            StandardOutTimer.Change(Timeout.Infinite, Timeout.Infinite);
            lock (CurrentProcess)
            {
                while (!CurrentProcess.StandardOutput.EndOfStream)
                    Write(CurrentProcess.StandardOutput.ReadLine());
                while (!CurrentProcess.StandardError.EndOfStream)
                    Write(CurrentProcess.StandardError.ReadLine());
                CurrentProcess = null;
            }
        }

        private void StandardOutTick(object discarded)
        {
            if (CurrentProcess == null)
                return;
            lock (CurrentProcess)
            {
                try
                {
                    while (!CurrentProcess.StandardOutput.EndOfStream)
                        Write(CurrentProcess.StandardOutput.ReadLine());
                    while (!CurrentProcess.StandardError.EndOfStream)
                        Write(CurrentProcess.StandardError.ReadLine());
                }
                catch { }
            }
        }

        public void Write(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Output.Count == 100)
                        Output.RemoveAt(0);
                    Output.Add(text);
                    scrollViewer.ScrollToBottom();
                }));
        }
    }
}
