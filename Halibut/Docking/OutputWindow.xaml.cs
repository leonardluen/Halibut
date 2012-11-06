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
using AvalonDock;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace Halibut.Docking
{
    /// <summary>
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : DockableContent
    {
        public OutputWindow()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    output.Text = "";
                }));
        }

        public void AddOutput(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    output.Text += text;
                    output.ScrollToEnd();
                }));
        }

        public void AddOutputLine(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                output.Text += text + Environment.NewLine;
                output.ScrollToEnd();
            }));
        }

        public void RunCommand(string command, string workingDirectory, Action<CommandResult> callback)
        {
            Clear();
            Task.Factory.StartNew(() =>
                {
                    try
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
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardError = true;
                        startInfo.WorkingDirectory = workingDirectory;
                        startInfo.CreateNoWindow = true;
                        var process = Process.Start(startInfo);
                        // TODO: Output as data comes in
                        string error = process.StandardError.ReadToEnd();
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        AddOutput(output);
                        AddOutput(error);
                        if (callback != null)
                        {
                            callback(new CommandResult
                                {
                                    Output = error + Environment.NewLine + output,
                                    ReturnCode = process.ExitCode
                                });
                        }
                    }
                    catch (Exception e)
                    {
                        if (callback != null)
                        {
                            callback(new CommandResult
                            {
                                ReturnCode = 1
                            });
                        }
                        AddOutputLine("Exception occured:");
                        AddOutputLine("Could not execute " + command);
                        AddOutput(e.ToString());
                    }
                });
        }

        private bool controlDown = false;
        private void output_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.HasFlag(Key.LeftCtrl) || e.Key.HasFlag(Key.RightCtrl))
                controlDown = true;
            if (!(controlDown && 
                (e.Key.HasFlag(Key.C) ||
                e.Key.HasFlag(Key.A)))) // Only allow copying
                e.Handled = true;
        }

        private void output_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.HasFlag(Key.LeftCtrl) || e.Key.HasFlag(Key.RightCtrl))
                controlDown = false;
        }

        public class CommandResult
        {
            public int ReturnCode { get; set; }
            public string Output { get; set; }
        }
    }
}
