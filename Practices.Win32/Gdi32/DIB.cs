using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Practices.Win32.Gdi32
{
    /// <summary>
    ///     DIB color table identifiers
    /// </summary>
    public enum DIB : int
    {
        RGB_COLORS      = 0, /* color table in RGBs */
        PAL_COLORS      = 1 /* color table in palette indices */
    }
}
