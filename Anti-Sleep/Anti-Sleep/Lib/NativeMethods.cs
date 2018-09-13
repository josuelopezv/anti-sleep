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
using System.Security;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace API.Native
{
	internal static partial class NativeMethods
	{
        [DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("User32.dll", SetLastError = true)]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern int GetWindowText(IntPtr hWnd,
			StringBuilder lpString, int nMaxCount);

        [DllImport("User32.dll")]
        internal static extern IntPtr GetMessageExtraInfo();

        [DllImport("User32.dll")]
		private static extern IntPtr GetForegroundWindow(); // Private, is wrapped

		
		[DllImport("User32.dll", EntryPoint = "SendInput", SetLastError = true)]
		internal static extern uint SendInput32(uint nInputs, INPUT32[] pInputs,
			int cbSize);

		[DllImport("User32.dll", EntryPoint = "SendInput", SetLastError = true)]
		internal static extern uint SendInput64Special(uint nInputs,
			SpecializedKeyboardINPUT64[] pInputs, int cbSize);

		[DllImport("User32.dll")]
		private static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("User32.dll")]
		private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType,
			IntPtr hKL);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		private static extern ushort VkKeyScan(char ch);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		private static extern ushort VkKeyScanEx(char ch, IntPtr hKL);

		[DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private static extern int ToUnicode(uint wVirtKey, uint wScanCode,
			IntPtr lpKeyState, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder sbBuff,
			int cchBuff, uint wFlags);

		[DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode,
			IntPtr lpKeyState, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder sbBuff,
			int cchBuff, uint wFlags, IntPtr hKL);

		[DllImport("User32.dll")]
		internal static extern ushort GetKeyState(int vKey);

		[DllImport("User32.dll")]
		internal static extern ushort GetAsyncKeyState(int vKey);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool BlockInput([MarshalAs(UnmanagedType.Bool)]
			bool fBlockIt);

		[DllImport("User32.dll")]
		internal static extern IntPtr GetKeyboardLayout(uint idThread);

		[DllImport("User32.dll")]
		internal static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint uFlags);

		[DllImport("User32.dll")]
		internal static extern uint GetWindowThreadProcessId(IntPtr hWnd,
			[Out] out uint lpdwProcessId);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] keyState);

    }
}
