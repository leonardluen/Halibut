using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Halibut
{
    public class BuildError
    {
        public string Error { get; set; }
        public string File { get; set; }
        public string FileFullPath { get; set; }
        public int LineNumber { get; set; }
    }
}
