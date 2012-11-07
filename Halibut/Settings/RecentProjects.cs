using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Halibut.Settings
{
    // TODO: Consider refactoring setting management after adding global settings
    public static class RecentProjects
    {
        public static List<Project> GetRecentProjects()
        {
            var recent = new List<Project>();
            if (!File.Exists(Path.Combine(MainWindow.UserPath, "Settings", "recent.xml")))
                return recent;
            var document = XDocument.Load(Path.Combine(MainWindow.UserPath, "Settings", "recent.xml"));
            foreach (var element in document.Root.Elements("project"))
            {
                try
                {
                    recent.Add(Project.FromFile(element.Attribute("config").Value));
                }
                catch { }
            }
            return recent;
        }

        public static void ProjectOpened(Project project)
        {
            XDocument document;
            if (!File.Exists(Path.Combine(MainWindow.UserPath, "Settings", "recent.xml")))
            {
                document = new XDocument();
                document.Add(new XElement("projects"));
            }
            else
                document = XDocument.Load(Path.Combine(MainWindow.UserPath, "Settings", "recent.xml"));
            var newProject = new XElement("project");
            newProject.SetAttributeValue("config", project.File);
            newProject.SetAttributeValue("name", project.Name);

            XElement toRemove = null;
            foreach (var element in document.Root.Elements())
            {
                if (element.Attribute("config").Value == element.Attribute("config").Value)
                {
                    toRemove = element;
                    break;
                }
            }
            if (toRemove != null)
                toRemove.Remove();

            document.Root.AddFirst(newProject);
            if (document.Root.Elements().Count() > 10)
            {
                var elements = document.Root.Elements().Take(10);
                document.Root.RemoveAll();
                document.Root.Add(elements);
            }
            document.Save(Path.Combine(MainWindow.UserPath, "Settings", "recent.xml"));
        }
    }
}
