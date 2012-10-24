using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halibut
{
    public interface IDirtiedWindow
    {
        string ObjectName { get; }
        bool IsDirty { get; }
        void Save();
    }
}
