/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2018 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

using System.Linq;
using System.Windows.Forms;

namespace API.Native
{
    internal static partial class NativeMethods
    {
        internal static string GetWindowText(IntPtr hWnd, bool bTrim)
        {
            int nLength = GetWindowTextLength(hWnd);
            if (nLength <= 0) return string.Empty;

            StringBuilder sb = new StringBuilder(nLength + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            string strWindow = sb.ToString();

            return (bTrim ? strWindow.Trim() : strWindow);
        }

        internal static IntPtr GetForegroundWindowHandle()
        {
            return GetForegroundWindow(); // Windows API


        }

        private static readonly char[] m_vWindowTrim = { '\r', '\n' };
        internal static void GetForegroundWindowInfo(out IntPtr hWnd,
            out string strWindowText, bool bTrimWindow)
        {
            hWnd = GetForegroundWindowHandle();

            strWindowText = GetWindowText(hWnd, bTrimWindow);

        }

        internal static uint? GetLastInputTime()
        {
            LASTINPUTINFO lii = new LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));

            if (!GetLastInputInfo(ref lii)) { Debug.Assert(false); return null; }

            return lii.dwTime;

        }


        /// <summary>
        /// PRIMARYLANGID macro.
        /// </summary>
        internal static ushort GetPrimaryLangID(ushort uLangID)
        {
            return (ushort)(uLangID & 0x3FFU);
        }

        internal static uint MapVirtualKey3(uint uCode, uint uMapType, IntPtr hKL)
        {
            if (hKL == IntPtr.Zero) return MapVirtualKey(uCode, uMapType);
            return MapVirtualKeyEx(uCode, uMapType, hKL);
        }

        internal static ushort VkKeyScan3(char ch, IntPtr hKL)
        {
            if (hKL == IntPtr.Zero) return VkKeyScan(ch);
            return VkKeyScanEx(ch, hKL);
        }

        /// <returns>
        /// Null, if there exists no translation or an error occured.
        /// An empty string, if the key is a dead key.
        /// Otherwise, the generated Unicode string (typically 1 character,
        /// but can be more when a dead key is stored in the keyboard layout).
        /// </returns>
        internal static string ToUnicode3(int vKey, byte[] pbKeyState, IntPtr hKL)
        {
            const int cbState = 256;
            IntPtr pState = IntPtr.Zero;
            try
            {
                uint uScanCode = MapVirtualKey3((uint)vKey, MAPVK_VK_TO_VSC, hKL);

                pState = Marshal.AllocHGlobal(cbState);
                if (pState == IntPtr.Zero) { Debug.Assert(false); return null; }

                if (pbKeyState != null)
                {
                    if (pbKeyState.Length == cbState)
                        Marshal.Copy(pbKeyState, 0, pState, cbState);
                    else { Debug.Assert(false); return null; }
                }
                else
                {
                    // Windows' GetKeyboardState function does not return
                    // the current virtual key array; as a workaround,
                    // calling GetKeyState is mentioned sometimes, but
                    // this doesn't work reliably either;
                    // http://pinvoke.net/default.aspx/user32/GetKeyboardState.html

                    // GetKeyState(VK_SHIFT);
                    // if(!GetKeyboardState(pState)) { Debug.Assert(false); return null; }

                    Debug.Assert(false);
                    return null;
                }

                const int cchUni = 30;
                StringBuilder sbUni = new StringBuilder(cchUni + 2);

                int r;
                if (hKL == IntPtr.Zero)
                    r = ToUnicode((uint)vKey, uScanCode, pState, sbUni,
                        cchUni, 0);
                else
                    r = ToUnicodeEx((uint)vKey, uScanCode, pState, sbUni,
                        cchUni, 0, hKL);

                if (r < 0) return string.Empty; // Dead key
                if (r == 0) return null; // No translation

                string str = sbUni.ToString();
                if (string.IsNullOrEmpty(str)) { Debug.Assert(false); return null; }

                // Extra characters may be returned, but are invalid
                // and should be ignored;
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms646320.aspx
                if (r < str.Length) str = str.Substring(0, r);

                return str;
            }
            catch (Exception) { Debug.Assert(false); }
            finally { if (pState != IntPtr.Zero) Marshal.FreeHGlobal(pState); }

            return null;
        }



        /// <summary>
        /// Gets all keys that are currently in the down state.
        /// </summary>
        /// <returns>
        /// A collection of all keys that are currently in the down state.
        /// </returns>
        public static bool isDownKeys()
        {
            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);
            return keyboardState.AsParallel().Any(x => x != 0);
        }
    }
}
