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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;


namespace API.Native
{
	internal sealed class SiEngineWin : SiEngineStd
	{
                
		private SiSendMethod? m_osmEnforced = null;
		
        private SiWindowInfo m_swiCurrent = new SiWindowInfo(IntPtr.Zero);

		private Keys m_kModCur = Keys.None;

        private static char[] m_vForcedUniChars = null;

        public override void SendKeyImpl(int iVKey, bool? bExtKey, bool? bDown)
		{
			if(bDown.HasValue)
			{
				SendVKeyNative(iVKey, bExtKey, bDown.Value);
				return;
			}
			SendVKeyNative(iVKey, bExtKey, true);
			SendVKeyNative(iVKey, bExtKey, false);
		}   

		private void SetKeyModifierImplEx(Keys kMod, bool bDown, bool bRAlt)
		{
			if((kMod & Keys.Shift) != Keys.None)
				SendVKeyNative((int)Keys.ShiftKey, null, bDown);
			if((kMod & Keys.Control) != Keys.None)
				SendVKeyNative((int)Keys.ControlKey, null, bDown);
			if((kMod & Keys.Alt) != Keys.None)
			{
				int vk = (int)(bRAlt ? Keys.RMenu : Keys.Menu);
				SendVKeyNative(vk, null, bDown);
			}

			if(bDown) m_kModCur |= kMod;
			else m_kModCur &= ~kMod;
		}

		public override void SendCharImpl(char ch, bool? bDown)
		{
			if(TrySendCharByKeypresses(ch, bDown)) return;

			if(bDown.HasValue)
			{
				SendCharNative(ch, bDown.Value);
				return;
			}

			SendCharNative(ch, true);
			SendCharNative(ch, false);
		}

		private bool SendVKeyNative(int vKey, bool? bExtKey, bool bDown)
		{
			bool bRes = false;

			if(IntPtr.Size == 4)
				bRes = SendVKeyNative32(vKey, bExtKey, null, bDown);
			else if(IntPtr.Size == 8)
				bRes = SendVKeyNative64(vKey, bExtKey, null, bDown);
			else { Debug.Assert(false); }

			// The following does not hold when sending keypresses to
			// key state-consuming windows (e.g. VM windows)
			// if(bDown && (vKey != NativeMethods.VK_CAPITAL))
			// {
			//	Debug.Assert(IsKeyActive(vKey));
			// }

			return bRes;
		}

		private bool SendCharNative(char ch, bool bDown)
		{
			if(IntPtr.Size == 4)
				return SendVKeyNative32(0, null, ch, bDown);
			else if(IntPtr.Size == 8)
				return SendVKeyNative64(0, null, ch, bDown);
			else { Debug.Assert(false); }

			return false;
		}

		private bool SendVKeyNative32(int vKey, bool? bExtKey, char? optUnicodeChar,
			bool bDown)
		{
			NativeMethods.INPUT32[] pInput = new NativeMethods.INPUT32[1];

			pInput[0].Type = NativeMethods.INPUT_KEYBOARD;

			if(optUnicodeChar.HasValue )
			{
				pInput[0].KeyboardInput.VirtualKeyCode = 0;
				pInput[0].KeyboardInput.ScanCode = (ushort)optUnicodeChar.Value;
				pInput[0].KeyboardInput.Flags = ((bDown ? 0 :
					NativeMethods.KEYEVENTF_KEYUP) | NativeMethods.KEYEVENTF_UNICODE);
			}
			else
			{
				IntPtr hKL = m_swiCurrent.KeyboardLayout;

				if(optUnicodeChar.HasValue)
					vKey = (int)(NativeMethods.VkKeyScan3(optUnicodeChar.Value,
						hKL) & 0xFFU);

				pInput[0].KeyboardInput.VirtualKeyCode = (ushort)vKey;
				pInput[0].KeyboardInput.ScanCode =
					(ushort)(NativeMethods.MapVirtualKey3((uint)vKey,
					NativeMethods.MAPVK_VK_TO_VSC, hKL) & 0xFFU);
				pInput[0].KeyboardInput.Flags = GetKeyEventFlags(vKey, bExtKey, bDown);
			}

			pInput[0].KeyboardInput.Time = 0;
			pInput[0].KeyboardInput.ExtraInfo = NativeMethods.GetMessageExtraInfo();

			Debug.Assert(Marshal.SizeOf(typeof(NativeMethods.INPUT32)) == 28);
			if(NativeMethods.SendInput32(1, pInput,
				Marshal.SizeOf(typeof(NativeMethods.INPUT32))) != 1)
				return false;

			return true;
		}

		private bool SendVKeyNative64(int vKey, bool? bExtKey, char? optUnicodeChar,
			bool bDown)
		{
			NativeMethods.SpecializedKeyboardINPUT64[] pInput = new
				NativeMethods.SpecializedKeyboardINPUT64[1];

			pInput[0].Type = NativeMethods.INPUT_KEYBOARD;

			if(optUnicodeChar.HasValue )
			{
				pInput[0].VirtualKeyCode = 0;
				pInput[0].ScanCode = (ushort)optUnicodeChar.Value;
				pInput[0].Flags = ((bDown ? 0 : NativeMethods.KEYEVENTF_KEYUP) |
					NativeMethods.KEYEVENTF_UNICODE);
			}
			else
			{
				IntPtr hKL = m_swiCurrent.KeyboardLayout;

				if(optUnicodeChar.HasValue)
					vKey = (int)(NativeMethods.VkKeyScan3(optUnicodeChar.Value,
						hKL) & 0xFFU);

				pInput[0].VirtualKeyCode = (ushort)vKey;
				pInput[0].ScanCode = (ushort)(NativeMethods.MapVirtualKey3(
					(uint)vKey, NativeMethods.MAPVK_VK_TO_VSC, hKL) & 0xFFU);
				pInput[0].Flags = GetKeyEventFlags(vKey, bExtKey, bDown);
			}

			pInput[0].Time = 0;
			pInput[0].ExtraInfo = NativeMethods.GetMessageExtraInfo();

			Debug.Assert(Marshal.SizeOf(typeof(NativeMethods.SpecializedKeyboardINPUT64)) == 40);
			if(NativeMethods.SendInput64Special(1, pInput,
				Marshal.SizeOf(typeof(NativeMethods.SpecializedKeyboardINPUT64))) != 1)
				return false;

			return true;
		}

		private static uint GetKeyEventFlags(int vKey, bool? bExtKey, bool bDown)
		{
			uint u = 0;

			if(!bDown) u |= NativeMethods.KEYEVENTF_KEYUP;

			if(bExtKey.HasValue)
			{
				if(bExtKey.Value) u |= NativeMethods.KEYEVENTF_EXTENDEDKEY;
			}
			else if(IsExtendedKeyEx(vKey))
				u |= NativeMethods.KEYEVENTF_EXTENDEDKEY;

			return u;
		}

		private static bool IsExtendedKeyEx(int vKey)
		{
#if DEBUG
			// https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731.aspx
			// https://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
			const uint m = NativeMethods.MAPVK_VK_TO_VSC;
			IntPtr h = IntPtr.Zero;
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_LSHIFT, m, h) == 0x2AU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_RSHIFT, m, h) == 0x36U);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_SHIFT, m, h) == 0x2AU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_LCONTROL, m, h) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_RCONTROL, m, h) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_CONTROL, m, h) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_LMENU, m, h) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_RMENU, m, h) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_MENU, m, h) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x5BU, m, h) == 0x5BU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x5CU, m, h) == 0x5CU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x5DU, m, h) == 0x5DU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6AU, m, h) == 0x37U);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6BU, m, h) == 0x4EU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6DU, m, h) == 0x4AU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6EU, m, h) == 0x53U);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6FU, m, h) == 0x35U);
#endif

			if((vKey >= 0x21) && (vKey <= 0x2E)) return true;
			if((vKey >= 0x5B) && (vKey <= 0x5D)) return true;
			if(vKey == 0x6F) return true; // VK_DIVIDE

			// RShift is separate; no E0
			if(vKey == NativeMethods.VK_RCONTROL) return true;
			if(vKey == NativeMethods.VK_RMENU) return true;

			return false;
		}
                
		private bool TrySendCharByKeypresses(char ch, bool? bDown)
		{
			if(ch == char.MinValue) { Debug.Assert(false); return false; }

			SiSendMethod sm = GetSendMethod(m_swiCurrent);
			if(sm == SiSendMethod.UnicodePacket) return false;

			if(m_vForcedUniChars == null)
				m_vForcedUniChars = new char[] {
					// All of the following diacritics are spacing / non-combining

					'\u005E', // Circumflex ^
					'\u0060', // Grave accent
					'\u00A8', // Diaeresis
					'\u00AF', // Macron above, long
					'\u00B0', // Degree (e.g. for Czech)
					'\u00B4', // Acute accent
					'\u00B8', // Cedilla

					// E.g. for US-International;
					// https://sourceforge.net/p/keepass/discussion/329220/thread/5708e5ef/
					'\u0022', // Quotation mark
					'\u0027', // Apostrophe
					'\u007E' // Tilde

					// Spacing Modifier Letters; see below
					// '\u02C7', // Caron (e.g. for Canadian Multilingual)
					// '\u02C9', // Macron above, modifier, short
					// '\u02CD', // Macron below, modifier, short
					// '\u02D8', // Breve
					// '\u02D9', // Dot above
					// '\u02DA', // Ring above
					// '\u02DB', // Ogonek
					// '\u02DC', // Small tilde
					// '\u02DD', // Double acute accent
				};
			if(sm != SiSendMethod.KeyEvent) // If Unicode packets allowed
			{
				if(Array.IndexOf<char>(m_vForcedUniChars, ch) >= 0) return false;

				// U+02B0 to U+02FF are Spacing Modifier Letters;
				// https://www.unicode.org/charts/PDF/U02B0.pdf
				// https://en.wikipedia.org/wiki/Spacing_Modifier_Letters
				if((ch >= '\u02B0') && (ch <= '\u02FF')) return false;
			}

			IntPtr hKL = m_swiCurrent.KeyboardLayout;
			ushort u = NativeMethods.VkKeyScan3(ch, hKL);
			if(u == 0xFFFFU) return false;

			int vKey = (int)(u & 0xFFU);

			Keys kMod = Keys.None;
			int nMods = 0;
			if((u & 0x100U) != 0U) { ++nMods; kMod |= Keys.Shift; }
			if((u & 0x200U) != 0U) { ++nMods; kMod |= Keys.Control; }
			if((u & 0x400U) != 0U) { ++nMods; kMod |= Keys.Alt; }
			if((u & 0x800U) != 0U) return false; // Hankaku unsupported

			// Do not send a key combination that is registered as hot key;
			// https://sourceforge.net/p/keepass/bugs/1235/
			// Windows shortcut hot keys involve at least 2 modifiers
			if(nMods >= 2)
			{
				Keys kFull = (kMod | (Keys)vKey);
				//if(HotKeyManager.IsHotKeyRegistered(kFull, true))
				//	return false;
			}

			// Windows' GetKeyboardState function does not return the
			// current virtual key array (especially not after changing
			// them below), thus we build the array on our own
			byte[] pbState = new byte[256];
			if((kMod & Keys.Shift) != Keys.None)
			{
				pbState[NativeMethods.VK_SHIFT] = 0x80;
				pbState[NativeMethods.VK_LSHIFT] = 0x80;
			}
			if((kMod & Keys.Control) != Keys.None)
			{
				pbState[NativeMethods.VK_CONTROL] = 0x80;
				pbState[NativeMethods.VK_LCONTROL] = 0x80;
			}
			if((kMod & Keys.Alt) != Keys.None)
			{
				pbState[NativeMethods.VK_MENU] = 0x80;
				pbState[NativeMethods.VK_RMENU] = 0x80; // See below
			}
			pbState[NativeMethods.VK_NUMLOCK] = 0x01; // Toggled

			bool bCapsLock = false;

			// The keypress that VkKeyScan returns may require a specific
			// state of toggle keys, on which it provides no information;
			// thus we now check whether the keypress will really result
			// in the character that we expect;
			// https://sourceforge.net/p/keepass/bugs/1594/
			string strUni = NativeMethods.ToUnicode3(vKey, pbState, hKL);
			if((strUni != null) && (strUni.Length == 0)) { } // Dead key
			else if(string.IsNullOrEmpty(strUni) || (strUni[strUni.Length - 1] != ch))
			{
				// Among the keyboard layouts that were tested, the
				// Czech one was the only one where the translation
				// may fail (due to dependency on the Caps Lock state)
				Debug.Assert(NativeMethods.GetPrimaryLangID((ushort)(hKL.ToInt64() &
					0xFFFFL)) == NativeMethods.LANG_CZECH);

				// Test whether Caps Lock is required
				pbState[NativeMethods.VK_CAPITAL] = 0x01;
				strUni = NativeMethods.ToUnicode3(vKey, pbState, hKL);
				if((strUni != null) && (strUni.Length == 0)) { } // Dead key
				else if(string.IsNullOrEmpty(strUni) || (strUni[strUni.Length - 1] != ch))
				{
					Debug.Assert(false); // An unknown key modifier is required
					return false;
				}

				bCapsLock = true;
			}

			if(bCapsLock)
			{
				SendKeyImpl(NativeMethods.VK_CAPITAL, null, null);
				Thread.Sleep(1);
				Application.DoEvents();
			}

			Keys kModDiff = (kMod & ~m_kModCur);
			if(kModDiff != Keys.None)
			{
				// Send RAlt for better AltGr compatibility;
				// https://sourceforge.net/p/keepass/bugs/1475/
				SetKeyModifierImplEx(kModDiff, true, true);
				Thread.Sleep(1);
				Application.DoEvents();
			}

			SendKeyImpl(vKey, null, bDown);

			if(kModDiff != Keys.None)
			{
				Thread.Sleep(1);
				Application.DoEvents();
				SetKeyModifierImplEx(kModDiff, false, true);
			}

			if(bCapsLock)
			{
				Thread.Sleep(1);
				Application.DoEvents();
				SendKeyImpl(NativeMethods.VK_CAPITAL, null, null);
			}

			return true;
		}

		private SiSendMethod GetSendMethod(SiWindowInfo swi)
		{
			if(m_osmEnforced.HasValue) return m_osmEnforced.Value;

			return swi.SendMethod;
		}

        

	}

}
