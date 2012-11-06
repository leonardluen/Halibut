using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Halibut
{
    public class ProjectTemplate
    {
        public ProjectTemplate(string templateFile)
        {
            XDocument document = XDocument.Load(templateFile);
            TemplateDirectory = Path.GetDirectoryName(templateFile);
            var name = document.Root.Attribute("name");
            if (name == null)
                throw new FileFormatException(Path.GetFileName(templateFile) + " is an invalid project template.");
            TemplateName = name.Value;
            var icon = document.Root.Attribute("icon");
            if (icon == null)
                throw new FileFormatException(Path.GetFileName(templateFile) + " is an invalid project template.");
            Icon = icon.Value;
            if (Path.IsPathRooted(Icon))
                Icon = Path.GetFullPath(Icon);
            else
                Icon = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), Icon));
            TemplateFiles = new List<TemplateFile>();
            foreach (var file in document.Root.Elements("file"))
            {
                var fileName = file.Attribute("name");
                var open = file.Attribute("open");
                var focus = file.Attribute("focus");
                var template = file.Attribute("template");
                if (fileName == null || template == null)
                    throw new FileFormatException(Path.GetFileName(templateFile) + " is an invalid project template.");
                var tFile = new TemplateFile();
                tFile.FileName = fileName.Value;
                tFile.Open = tFile.Focused = false;
                tFile.Template = template.Value;
                if (open != null)
                    tFile.Open = bool.Parse(open.Value);
                if (focus != null)
                    tFile.Focused = bool.Parse(open.Value);
                TemplateFiles.Add(tFile);
            }
        }

        public string TemplateName { get; set; }
        public List<TemplateFile> TemplateFiles { get; set; }
        public string Icon { get; set; }
        public string TemplateDirectory { get; set; }

        public Project Create(string name, string directory)
        {
            var project = new Project();
            Directory.CreateDirectory(directory);
            project.RootDirectory = directory;
            foreach (var file in TemplateFiles)
            {
                string templateFile = file.Template;
                if (Path.IsPathRooted(templateFile))
                    templateFile = Path.GetFullPath(templateFile);
                else
                    templateFile = Path.GetFullPath(Path.Combine(TemplateDirectory, templateFile));
                string template = File.ReadAllText(templateFile);
                template = DoReplacements(name, template);
                var output = Path.Combine(project.RootDirectory,
                    DoReplacements(name, file.FileName));
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllText(output, template);
            }
            return project;
        }

        public string DoReplacements(string name, string template)
        {
            return template.Replace("{project-name}", name)
                .Replace("{project-fname}", NewProjectWindow.GetNameAsFile(name))
                .Replace("{current-date}", DateTime.Now.ToLongDateString())
                .Replace("{{", "{").Replace("}}", "}"); // TODO: More of these
        }

        public class TemplateFile
        {
            public string FileName { get; set; }
            public string Template { get; set; }
            public bool Open { get; set; }
            public bool Focused { get; set; }
        }
    }
}
