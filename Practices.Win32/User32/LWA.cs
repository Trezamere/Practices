using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Practices.Win32.User32
{
    /// <summary>
    ///     Flags for SetLayeredWindowAttributes
    /// </summary>
    [Flags]
    public enum LWA : int
    {
        COLORKEY = 0x1,
        ALPHA = 0x2
    }
}
