using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Halibut
{
    /// <summary>
    /// Used to manage the current project enviornment
    /// </summary>
    public class EnviornmentConfiguration
    {
        public string File { get; set; }

        private Dictionary<string, string> Values { get; set; }

        public EnviornmentConfiguration()
        {
            Values = new Dictionary<string, string>();
        }

        public void Save()
        {
            Save(File);
        }

        public void Save(string file)
        {
            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.WriteLine("# enviornment.config");
                writer.WriteLine("# Last modified " + DateTime.Now.ToLongDateString() + " by Halibut");
                writer.WriteLine();
                foreach (var pair in Values)
                    writer.WriteLine(pair.Key + "=" + pair.Value);
                writer.Close();
            }
        }

        public string this[string key]
        {
            get
            {
                key = FormatKey(key);
                if (Values.ContainsKey(key))
                    return Values[key];
                return null;
            }
            set
            {
                Values[FormatKey(key)] = value;
            }
        }

        private string FormatKey(string key)
        {
            return key.Replace(" ", "-"); // TODO: More sanitation
        }
    }
}
