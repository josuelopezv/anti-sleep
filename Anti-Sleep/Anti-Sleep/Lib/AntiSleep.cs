using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace anti_sleep
{
    public class AntiSleep
    {
        public int timeoutInSecs = 10;
        private System.Threading.Thread t = null;

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    if (enabled) Start(); else Stop();
                }
            }
        }


        private void Start()
        {
            if (t == null)
            {
                //NativeMethods.SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED);
                t = new System.Threading.Thread(new System.Threading.ThreadStart(() => startSync()));
            }
            t.Start();
        }

        private void Stop()
        {
            t?.Abort();
        }

        private void startSync()
        {
            do
            {
                if (isSamePos())
                {
                    wakeCPU();
                    sleep();
                }
            } while (true);
        }

        private void sleep()
        {
            System.Threading.Thread.Sleep(timeoutInSecs * 1000);
        }

        private bool isSamePos()
        {
            var p = NativeMethods.GetCursorPosition();
            sleep();
            var p2 = NativeMethods.GetCursorPosition();
            return (p.X == p2.X && p.Y == p2.Y);
        }

        public void wakeCPU()
        {
            if (NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.ES_CONTINUOUS
                | NativeMethods.EXECUTION_STATE.ES_DISPLAY_REQUIRED
                | NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED
                | NativeMethods.EXECUTION_STATE.ES_AWAYMODE_REQUIRED) == 0) //Away mode for Windows >= Vista
                NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.ES_CONTINUOUS
                    | NativeMethods.EXECUTION_STATE.ES_DISPLAY_REQUIRED
                    | NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED); //Windows < Vista, forget away mode
        }


        internal class NativeMethods
        {
            /// <summary>
            /// Struct representing a point.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            internal struct POINT
            {
                public int X;
                public int Y;
                public static implicit operator Point(POINT point)
                {
                    return new Point(point.X, point.Y);
                }
            }

            /// <summary>
            /// Retrieves the cursor's position, in screen coordinates.
            /// </summary>
            /// <see>See MSDN documentation for further information.</see>
            [DllImport("user32.dll")]
            internal static extern bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll")]
            internal static extern bool SetCursorPos(int x, int y);

            internal static Point GetCursorPosition()
            {
                POINT lpPoint;
                GetCursorPos(out lpPoint);
                //bool success = User32.GetCursorPos(out lpPoint);
                // if (!success)

                return lpPoint;
            }


            internal enum EXECUTION_STATE : uint
            {
                ES_SYSTEM_REQUIRED = 0x00000001,
                ES_AWAYMODE_REQUIRED = 0x00000040,
                ES_CONTINUOUS = 0x80000000,
                ES_DISPLAY_REQUIRED = 0x00000002,
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        }


    }
}
