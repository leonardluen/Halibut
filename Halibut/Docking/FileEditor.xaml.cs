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
using ICSharpCode.AvalonEdit.Highlighting;
using AvalonDock;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
using System.Reflection;
using Microsoft.Win32;
using System.ComponentModel;

namespace Halibut.Docking
{
    /// <summary>
    /// Interaction logic for FileEditor.xaml
    /// </summary>
    public partial class FileEditor : DockableContent, IDirtiedWindow
    {
        public const string FileFilter = "DCPU-16 Assembly (*.dasm)|*.dasm|All files (*.*)|*.*";

        internal static void Initialize()
        {
            HighlightingManager.Instance.RegisterHighlighting("DCPU-16 Assembly", new[] { ".dasm", ".dasm16", ".dcpu", ".dcpu16" },
                HighlightingLoader.Load(FromResource("Halibut.Highlighting.dasm.xshd"), HighlightingManager.Instance));
            HighlightingManager.Instance.RegisterHighlighting("Markdown", new[] { ".md", ".markdown", ".mkdown" },
                HighlightingLoader.Load(FromResource("Halibut.Highlighting.markdown.xshd"), HighlightingManager.Instance));
            HighlightingManager.Instance.RegisterHighlighting("z80 Assembly", new[] { ".z80", ".asm" },
                HighlightingLoader.Load(FromResource("Halibut.Highlighting.z80.xshd"), HighlightingManager.Instance));
            foreach (var name in HighlightingManager.Instance.HighlightingNames)
                Console.WriteLine(name);
        }

        private static XmlReader FromResource(string resource)
        {
            return new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resource));
        }

        public string ObjectName 
        {
            get
            {
                if (FileName == null)
                    return "New File";
                return Path.GetFileName(FileName);
            }
        }

        public string FileName { get; set; }

        public bool IsDirty { get; private set; }

        public FileEditor(string file)
        {
            InitializeComponent();
            FileName = file;
            if (file != null)
            {
                textEditor.SyntaxHighlighting =
                    HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(file));
                var ext = Path.GetExtension(FileName);
                textEditor.WordWrap = ext == ".md" || ext == ".markdown" || ext == ".mkdown";
                textEditor.Document.Text = File.ReadAllText(file);
                Title = Path.GetFileName(file);
            }
            else
                Title = "New File";
            IsDirty = false;
            this.IsCloseable = true;
            this.HideOnClose = false;
            Closing += FileEditor_Closing;
            // TODO: Allow customization
            textEditor.Options.ConvertTabsToSpaces = true;
            textEditor.Options.AllowScrollBelowDocument = true;
            textEditor.Options.EnableRectangularSelection = true;
            textEditor.Options.IndentationSize = 4;

            textEditor.PreviewMouseWheel += textEditor_PreviewMouseWheel;
            textEditor.PreviewKeyDown += textEditor_PreviewKeyDown;
            textEditor.PreviewKeyUp += textEditor_PreviewKeyUp;
        }

        bool isCtrlPressed = false;
        void textEditor_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (isCtrlPressed && e.Key == Key.RightCtrl || e.Key == Key.LeftCtrl)
                isCtrlPressed = false;
        }

        void textEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            isCtrlPressed = e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl;
        }

        void textEditor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (isCtrlPressed)
            {
                if (e.Delta > 0 && textEditor.FontSize < 120)
                    textEditor.FontSize++;
                else if (e.Delta < 0 && textEditor.FontSize > 8)
                    textEditor.FontSize--;
                e.Handled = true;
            }
        }

        public void Save()
        {
            // TODO: Set save file dialog path relative to current configuration (if appliciable)
            if (FileName == null)
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = FileFilter;
                if (!dialog.ShowDialog().Value)
                    return;
                FileName = dialog.FileName;
                textEditor.SyntaxHighlighting =
                    HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(FileName));
                var ext = Path.GetExtension(FileName);
                textEditor.WordWrap = ext == "md" || ext == "markdown" || ext == "mkdown";
            }
            File.WriteAllText(FileName, textEditor.Text);
            IsDirty = false;
            Title = ObjectName;
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
            Title = ObjectName + "*";
        }

        private void FileEditor_Closing(object sender, CancelEventArgs e)
        {
            if (IsDirty)
            {
                var result = MessageBox.Show(ObjectName + " has been modified! Are you sure you wish to discard your changes?",
                    "Warning", MessageBoxButton.YesNoCancel);
                if (result != MessageBoxResult.Yes)
                    e.Cancel = true;
            }
        }
    }
}
