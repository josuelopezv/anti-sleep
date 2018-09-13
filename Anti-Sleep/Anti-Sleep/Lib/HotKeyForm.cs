using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using API.Native;

namespace API
{
    public class HotKeyForm : Form
    {
        private List<HotKey> HotKeys { get; set; } = new List<HotKey>();

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == NativeMethods.WM_HOTKEY)
            {
                if (isBusy) return;
                isBusy = true;
                // get the keys.
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                ModKeys modifier = (ModKeys)((int)m.LParam & 0xFFFF);

                var hk = HotKeys.Where(x => x.Modifier == modifier && x.Key == key).FirstOrDefault();
                System.Diagnostics.Debug.Assert(hk != null, $"Hot key ({modifier} + {key}) received but not found a valid registered hot key");

                //while (NativeMethods.isDownKeys())
                //    System.Threading.Thread.Sleep(200);
                SendTextToExternal(hk);
                isBusy = false;
            }
        }
        private static bool isBusy = false;

        private void SendTextToExternal(HotKey hk)
        {
            VirtualKeyboard.Send(hk.Text, hk.IsMacro);
        }

        public void RegisterHotKey(HotKey hk)
        {
            if (!NativeMethods.RegisterHotKey(this.Handle, HotKeys.Count, (uint)hk.Modifier, (uint)hk.Key))
                throw new InvalidOperationException("Couldn’t register the hot key.");
            HotKeys.Add(hk);
        }

        public void RegisterAndReplace(IEnumerable<HotKey> lhk)
        {
            UnRegisterAll();
            foreach (var item in lhk)
            {
                RegisterHotKey(item);
            }
        }

        public void UnRegisterAll()
        {
            for (int i = 0; i < HotKeys.Count; i++)
            {
                NativeMethods.UnregisterHotKey(this.Handle, i);
            }
            HotKeys.Clear();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnRegisterAll();
            base.OnHandleDestroyed(e);
        }
    }
}
