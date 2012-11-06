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
using System.IO;

namespace Halibut
{
    /// <summary>
    /// Interaction logic for NewProjectWindow.xaml
    /// </summary>
    public partial class NewProjectWindow : Window
    {
        public List<ProjectTemplate> Templates { get; set; }
        public ProjectTemplate SelectedTemplate { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDirectory { get; set; }
        private bool projectNameModified { get; set; }

        public NewProjectWindow()
        {
            InitializeComponent();
            GetTemplates();
            templateList.ItemsSource = Templates;
            templateList.SelectionChanged += TemplateListOnSelectionChanged;
            targetDirectory.Text = Path.Combine(MainWindow.UserPath, "Projects");
            projectNameModified = false;
        }

        private void TemplateListOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (templateList.SelectedIndex != -1 && !projectNameModified)
            {
                var template = templateList.SelectedItem as ProjectTemplate;
                var name = GetNameAsFile(template.TemplateName);
                int i = 1;
                while (Directory.Exists(Path.Combine(MainWindow.UserPath, "Projects", name + i)))
                    i++;
                name += i;
                projectName.Text = name;
            }
            okButton.IsEnabled = IsValid();
        }

        public static string GetNameAsFile(string name)
        {
            name = name.Replace(" ", "");
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "");
            return name;
        }

        private void GetTemplates()
        {
            var templates = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Templates"),
                "*.template", SearchOption.AllDirectories);
            templates = templates.Concat(Directory.GetFiles(Path.Combine(MainWindow.UserPath, "Templates"),
                "*.template", SearchOption.AllDirectories)).ToArray();
            Templates = new List<ProjectTemplate>();
            foreach (var template in templates)
                Templates.Add(new ProjectTemplate(template));
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTemplate = (ProjectTemplate)templateList.SelectedItem;
            ProjectName = projectName.Text;
            ProjectDirectory = targetDirectory.Text;
            DialogResult = true;
            Close();
        }

        private void projectName_TextChanged(object sender, TextChangedEventArgs e)
        {
            projectNameModified = true;
            okButton.IsEnabled = IsValid();
        }

        private bool IsValid()
        {
            return projectName.Text.Length != 0 &&
                targetDirectory.Text.Length != 0 &&
                templateList.SelectedIndex != -1;
        }

        private void targetDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            okButton.IsEnabled = IsValid();
        }
    }
}
