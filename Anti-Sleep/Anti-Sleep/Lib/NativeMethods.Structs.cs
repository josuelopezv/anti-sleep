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
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace API.Native
{
	internal static partial class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct MOUSEINPUT32_WithSkip
		{
			public uint __Unused0; // See INPUT32 structure

			public int X;
			public int Y;
			public uint MouseData;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct KEYBDINPUT32_WithSkip
		{
			public uint __Unused0; // See INPUT32 structure

			public ushort VirtualKeyCode;
			public ushort ScanCode;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct INPUT32
		{
			[FieldOffset(0)]
			public uint Type;
			[FieldOffset(0)]
			public MOUSEINPUT32_WithSkip MouseInput;
			[FieldOffset(0)]
			public KEYBDINPUT32_WithSkip KeyboardInput;
		}

		// INPUT.KI (40). vk: 8, sc: 10, fl: 12, t: 16, ex: 24
		[StructLayout(LayoutKind.Explicit, Size = 40)]
		internal struct SpecializedKeyboardINPUT64
		{
			[FieldOffset(0)]
			public uint Type;
			[FieldOffset(8)]
			public ushort VirtualKeyCode;
			[FieldOffset(10)]
			public ushort ScanCode;
			[FieldOffset(12)]
			public uint Flags;
			[FieldOffset(16)]
			public uint Time;
			[FieldOffset(24)]
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LASTINPUTINFO
		{
			public uint cbSize;
			public uint dwTime;
		}

		internal const uint PROCESSENTRY32SizeUni32 = 556;
		internal const uint PROCESSENTRY32SizeUni64 = 568;

		internal const uint ACTCTXSize32 = 32;
		internal const uint ACTCTXSize64 = 56;
        
	}
}
