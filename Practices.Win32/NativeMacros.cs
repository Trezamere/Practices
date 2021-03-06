﻿using Practices.Win32.Gdi32;

namespace Practices.Win32
{
    public static class NativeMacros
    {
        public static ushort HIWORD(uint dword)
        {
            return (ushort)((dword >> 16) & 0xFFFF);
        }

        public static ushort LOWORD(uint dword)
        {
            return (ushort)dword;
        }

        public static int GET_X_LPARAM(uint dword)
        {
            return unchecked((int)(short)LOWORD(dword));
        }

        public static int GET_Y_LPARAM(uint dword)
        {
            return unchecked((int)(short)HIWORD(dword));
        }

        public static COLORREF RGB(byte r, byte g, byte b)
        {
            return new COLORREF(r, g, b);
        }
    }
}
