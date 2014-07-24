using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Practices.Win32.User32
{
    /// <summary>
    ///     WM_MOUSEACTIVATE Return Codes
    /// </summary>
    public enum MA : int
    {
        ACTIVATE = 1,
        ACTIVATEANDEAT = 2,
        NOACTIVATE = 3,
        NOACTIVATEANDEAT = 4
    }
}
