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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using API.Native;

namespace API
{
    public static class HotKeyManager
    {
        private static Form m_fRecvWnd = null;
        private static Dictionary<int, Keys> m_vRegKeys = new Dictionary<int, Keys>();

        public static bool Initialize(Form fRecvWnd)
        {
            m_fRecvWnd = fRecvWnd;


            return true;
        }

        public static bool RegisterHotKey(int nId, Keys kKey)
        {
            UnregisterHotKey(nId);

            uint uMod = 0;
            if ((kKey & Keys.Shift) != Keys.None) uMod |= NativeMethods.MOD_SHIFT;
            if ((kKey & Keys.Alt) != Keys.None) uMod |= NativeMethods.MOD_ALT;
            if ((kKey & Keys.Control) != Keys.None) uMod |= NativeMethods.MOD_CONTROL;

            uint vkCode = (uint)(kKey & Keys.KeyCode);
            if (vkCode == (uint)Keys.None) return false; // Don't register mod keys only

            try
            {
                if (NativeMethods.RegisterHotKey(m_fRecvWnd.Handle, nId, uMod, vkCode))
                {
                    m_vRegKeys[nId] = kKey;
                    return true;
                }

            }
            catch (Exception) { Debug.Assert(false); }

            return false;
        }

        public static bool UnregisterHotKey(int nId)
        {
            if (m_vRegKeys.ContainsKey(nId))
            {
                // Keys k = m_vRegKeys[nId];
                m_vRegKeys.Remove(nId);

                try
                {
                    bool bResult;
                        bResult = NativeMethods.UnregisterHotKey(m_fRecvWnd.Handle, nId);

                    // Debug.Assert(bResult);
                    return bResult;
                }
                catch (Exception) { Debug.Assert(false); }
            }

            return false;
        }

        public static void UnregisterAll()
        {
            List<int> vIDs = new List<int>(m_vRegKeys.Keys);
            foreach (int nID in vIDs) UnregisterHotKey(nID);

            Debug.Assert(m_vRegKeys.Count == 0);
        }

        public static bool IsHotKeyRegistered(Keys kKey, bool bGlobal)
        {
            if (m_vRegKeys.ContainsValue(kKey)) return true;
            if (!bGlobal) return false;

            int nID = 999;
            if (!RegisterHotKey(nID, kKey)) return true;

            UnregisterHotKey(nID);
            return false;
        }

        
    }
}
