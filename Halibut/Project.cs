using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Halibut
{
    public class Project
    {
        private Dictionary<string, string> Values { get; set; }
        public string RootDirectory { get; set; }
        public string File { get; set; }
        public string Name 
        {
            get
            {
                return this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        public static Project FromFile(string file)
        {
            var project = new Project();
            project.Values = new Dictionary<string, string>();
            project.File = file;
            project.RootDirectory = Path.GetDirectoryName(file);
            
            // Open and parse project config
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    line = line.Trim();
                    if (line.StartsWith("#"))
                        continue;
                    if (!line.Contains("="))
                        continue;
                    var key = line.Remove(line.IndexOf('='));
                    var value = line.Substring(line.IndexOf('=') + 1);
                    project.Values[key] = value;
                }
            }
            return project;
        }

        public string this[string key]
        {
            get { return Values[key];  }
            set { Values[key] = value; }
        }

        public bool ContainsKey(string key)
        {
            return Values.ContainsKey(key);
        }
    }
}
