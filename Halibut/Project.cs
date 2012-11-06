using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Halibut
{
    public class Project
    {
        public string Name { get; set; }
        public string RootDirectory { get; set; }
        public string BuildCommand { get; set; }
        public Regex ErrorRegex { get; set; }
        public string DebugCommand { get; set; }
        public int DebugPort { get; set; }
    }
}
