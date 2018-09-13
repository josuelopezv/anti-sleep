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
using System.Diagnostics;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;



namespace API.Native
{
	internal abstract class SiEngineStd 
	{
		protected bool Cancelled = false;

		private Stopwatch m_swLastEvent = new Stopwatch();

		public abstract void SendKeyImpl(int iVKey, bool? bExtKey, bool? bDown);
		public abstract void SendCharImpl(char ch, bool? bDown);

		private bool PreSendEvent()
		{
			// Update event time *before* actually performing the event
			m_swLastEvent.Reset();
			m_swLastEvent.Start();

			return ValidateState();
		}

		public void SendKey(int iVKey, bool? bExtKey, bool? bDown)
		{
			if(!PreSendEvent()) return;

			SendKeyImpl(iVKey, bExtKey, bDown);

			Application.DoEvents();
		}
        
		public void SendChar(char ch, bool? bDown)
		{
			if(!PreSendEvent()) return;

			SendCharImpl(ch, bDown);

			Application.DoEvents();
		}

		public virtual void Delay(uint uMs)
		{
			if(this.Cancelled) return;

			if(!m_swLastEvent.IsRunning)
			{
				Thread.Sleep((int)uMs);
				m_swLastEvent.Reset();
				m_swLastEvent.Start();
				return;
			}

			m_swLastEvent.Stop();
			long lAlreadyDelayed = m_swLastEvent.ElapsedMilliseconds;
			long lRemDelay = (long)uMs - lAlreadyDelayed;

			if(lRemDelay >= 0) Thread.Sleep((int)lRemDelay);

			m_swLastEvent.Reset();
			m_swLastEvent.Start();
		}

		private bool ValidateState()
		{
			if(this.Cancelled) return false;
			return true;
		}
	}
}
