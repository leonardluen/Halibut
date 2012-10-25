using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Halibut
{
    public static class DataUtility
    {
        public static bool IsPlaintext(string path)
        {
            try
            {
                // TODO: See if there's a better way to do this.
                var streamReader = new StreamReader(path);
                streamReader.ReadToEnd();
                streamReader.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
