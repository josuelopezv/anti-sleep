

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace API.Native
{
    public static class VirtualKeyboard
    {

        #region Local variables
        private static Hashtable keywords;
        static readonly SiEngineWin siEngineWin = new SiEngineWin();
        #endregion

        static VirtualKeyboard()
        {
            keywords = new Hashtable
            {
                { "SLEEP", -99 },
                { "BACKSPACE", (int)Keys.Back },
                { "BS", (int)Keys.Back },
                { "BKSP", (int)Keys.Back },
                { "BREAK", (int)Keys.Cancel },
                { "CAPSLOCK", (int)Keys.CapsLock },
                { "DELETE", (int)Keys.Delete },
                { "DEL", (int)Keys.Delete },
                { "DOWN", (int)Keys.Down },
                { "END", (int)Keys.End },
                { "ENTER", (int)Keys.Enter },
                { "~", (int)Keys.Enter },
                { "ESC", (int)Keys.Escape },
                { "HELP", (int)Keys.Help },
                { "HOME", (int)Keys.Home },
                { "INSERT", (int)Keys.Insert },
                { "INS", (int)Keys.Insert },
                { "LEFT", (int)Keys.Left },
                { "NUMLOCK", (int)Keys.NumLock },
                { "PGDN", (int)Keys.PageDown },
                { "PGUP", (int)Keys.PageUp },
                { "PRTSC", (int)Keys.PrintScreen },
                { "RIGHT", (int)Keys.Right },
                { "SCROLLLOCK", (int)Keys.Scroll },
                { "TAB", (int)Keys.Tab },
                { "UP", (int)Keys.Up },
                { "F1", (int)Keys.F1 },
                { "F2", (int)Keys.F2 },
                { "F3", (int)Keys.F3 },
                { "F4", (int)Keys.F4 },
                { "F5", (int)Keys.F5 },
                { "F6", (int)Keys.F6 },
                { "F7", (int)Keys.F7 },
                { "F8", (int)Keys.F8 },
                { "F9", (int)Keys.F9 },
                { "F10", (int)Keys.F10 },
                { "F11", (int)Keys.F11 },
                { "F12", (int)Keys.F12 },
                { "F13", (int)Keys.F13 },
                { "F14", (int)Keys.F14 },
                { "F15", (int)Keys.F15 },
                { "F16", (int)Keys.F16 },
                { "ADD", (int)Keys.Add },
                { "SUBTRACT", (int)Keys.Subtract },
                { "MULTIPLY", (int)Keys.Multiply },
                { "DIVIDE", (int)Keys.Divide },
                { "+", (int)Keys.ShiftKey },
                { "^", (int)Keys.ControlKey },
                { "%", (int)Keys.Menu }
            };
        }

        #region Private methods

        private static SiEvent GetCharEvent(char key)
        {
            return new SiEvent { Type = SiEventType.Char, Char = key };

        }

        private static SiEvent GetKeyEvent(int k, bool? down = null)
        {
            if (k == (int)keywords["SLEEP"])
            {
                return new SiEvent { Type = SiEventType.Delay, Delay = 50 };
            }
            return new SiEvent { Type = SiEventType.Key, VKey = k, Down = down };
        }

        private static IEnumerable<SiEvent> GetKeyEvent(int k, int repeat_count)
        {
            for (int i = 0; i < repeat_count; i++)
                yield return GetKeyEvent(k);
        }

        private static List<SiEvent> Parse(string key_string)
        {
            bool isBlock = false;
            bool isVkey = false;
            bool isRepeat = false;
            bool isShift = false;
            bool isCtrl = false;
            bool isAlt = false;

            var lret = new List<SiEvent>();

            StringBuilder repeats = new StringBuilder();
            StringBuilder group_string = new StringBuilder();

            int key_len = key_string.Length;
            for (int i = 0; i < key_len; i++)
            {
                switch (key_string[i])
                {
                    case '{':
                        group_string.Remove(0, group_string.Length);
                        repeats.Remove(0, repeats.Length);
                        int start = i + 1;
                        for (; start < key_len && key_string[start] != '}'; start++)
                        {
                            if (Char.IsWhiteSpace(key_string[start]))
                            {
                                if (isRepeat)
                                    throw new ArgumentException("SendKeys string {0} is not valid.", key_string);

                                isRepeat = true;
                                continue;
                            }
                            if (isRepeat)
                            {
                                if (!Char.IsDigit(key_string[start]))
                                    throw new ArgumentException("SendKeys string {0} is not valid.", key_string);

                                repeats.Append(key_string[start]);

                                continue;
                            }

                            group_string.Append(key_string[start]);
                        }
                        if (start == key_len || start == i + 1)
                            throw new ArgumentException("SendKeys string {0} is not valid.", key_string);

                        else if (keywords.Contains(group_string.ToString().ToUpper()))
                        {
                            isVkey = true;
                        }
                        else
                        {
                            throw new ArgumentException("SendKeys string {0} is not valid.", key_string);
                        }

                        int repeat = 1;
                        if (repeats.Length > 0)
                            repeat = Int32.Parse(repeats.ToString());
                        if (isVkey)
                            lret.AddRange(GetKeyEvent((int)keywords[group_string.ToString().ToUpper()], repeat));
                        else
                        {
                            for (int count = 0; count < repeat; count++)
                            {
                                lret.Add(GetCharEvent(Char.Parse(group_string.ToString().ToUpper())));
                            }
                            //if (Char.IsUpper(Char.Parse(group_string.ToString())))
                            //{
                            //    if (!isShift)
                            //        lret.Add(GetKeyEvent((int)keywords["+"], true));
                            //    lret.Add(GetKeyEvent(Char.Parse(group_string.ToString())));
                            //    if (!isShift)
                            //        lret.Add(GetKeyEvent((int)keywords["+"], false));
                            //}
                            //else
                            //    lret.AddRange(GetKeyEvent(Char.Parse(group_string.ToString().ToUpper()), repeat));
                        }

                        i = start;
                        isRepeat = isVkey = false;
                        if (isShift)
                            lret.Add(GetKeyEvent((int)keywords["+"], false));
                        if (isCtrl)
                            lret.Add(GetKeyEvent((int)keywords["^"], false));
                        if (isAlt)
                            lret.Add(GetKeyEvent((int)keywords["%"], false));
                        isShift = isCtrl = isAlt = false;
                        break;

                    case '+':
                        lret.Add(GetKeyEvent((int)keywords["+"], true));
                        isShift = true; ;
                        break;

                    case '^':
                        lret.Add(GetKeyEvent((int)keywords["^"], true));
                        isCtrl = true;
                        break;

                    case '%':
                        lret.Add(GetKeyEvent((int)keywords["%"], true));
                        isAlt = true;
                        break;
                    case '~':
                        lret.Add(GetKeyEvent((int)keywords["ENTER"]));
                        break;
                    case '(':
                        isBlock = true;
                        break;
                    case ')':
                        if (isShift)
                            lret.Add(GetKeyEvent((int)keywords["+"], false));
                        if (isCtrl)
                            lret.Add(GetKeyEvent((int)keywords["^"], false));
                        if (isAlt)
                            lret.Add(GetKeyEvent((int)keywords["%"], false));
                        isShift = isCtrl = isAlt = isBlock = false;
                        break;
                    default:
                        //if (Char.IsUpper(key_string[i]))
                        //{
                        //    if (!isShift)
                        //        lret.Add(GetKeyEvent((int)keywords["+"], true));
                        //    lret.Add(GetKeyEvent(key_string[i]));
                        //    if (!isShift)
                        //        lret.Add(GetKeyEvent((int)keywords["+"], false));
                        //}
                        //else
                        lret.Add(GetCharEvent(key_string[i]));
                        if (!isBlock)
                        {
                            if (isShift)
                                lret.Add(GetKeyEvent((int)keywords["+"], false));
                            if (isCtrl)
                                lret.Add(GetKeyEvent((int)keywords["^"], false));
                            if (isAlt)
                                lret.Add(GetKeyEvent((int)keywords["%"], false));
                            isShift = isCtrl = isAlt = isBlock = false;
                        }
                        break;

                }
            }

            if (isBlock)
                throw new ArgumentException("SendKeys string {0} is not valid.", key_string);

            if (isShift)
                lret.Add(GetKeyEvent((int)keywords["+"], false));
            if (isCtrl)
                lret.Add(GetKeyEvent((int)keywords["^"], false));
            if (isAlt)
                lret.Add(GetKeyEvent((int)keywords["%"], false));

            return lret;
        }

        private static void SendPriv(List<SiEvent> l)
        {
            uint uDefaultDelay = 35;

            bool bFirstInput = true;
            foreach (SiEvent si in l)
            {
                if ((si.Type == SiEventType.Key) || (si.Type == SiEventType.Char))
                {
                    if (!bFirstInput)
                        siEngineWin.Delay(uDefaultDelay);

                    bFirstInput = false;
                }

                switch (si.Type)
                {
                    case SiEventType.Key:
                        siEngineWin.SendKey(si.VKey, si.ExtendedKey, si.Down);
                        break;

                    case SiEventType.Char:
                        siEngineWin.SendChar(si.Char, si.Down);
                        break;

                    case SiEventType.Delay:
                        siEngineWin.Delay(si.Delay);
                        break;

                    default:
                        throw new Exception("Key event error: not found");
                }

                // Extra delay after tabs
                if (((si.Type == SiEventType.Key) && (si.VKey == (int)Keys.Tab)) ||
                    ((si.Type == SiEventType.Char) && (si.Char == '\t')))
                {
                    if (uDefaultDelay < 100)
                        siEngineWin.Delay(uDefaultDelay);
                }
            }
        }


        #endregion // Private Methods

        #region Public Static Methods
        public static void Send(string keys, bool parse)
        {
            System.Threading.Thread.Sleep(500);
            List<SiEvent> levents = parse ? Parse(keys) : keys.ToArray().Select(x => GetCharEvent(x)).ToList();
            SendPriv(levents);
        }
        #endregion // Public Static Methods
    }

}