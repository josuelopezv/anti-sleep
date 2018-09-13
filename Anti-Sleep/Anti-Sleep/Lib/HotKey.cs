using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace API
{
    public class HotKey
    {
        public static readonly IEnumerable<Keys> modifiers = new[] { Keys.Alt, Keys.Control, Keys.Shift, Keys.LWin };
        public string Text { get; set; }

        public bool IsMacro { get; set; }

        public ModKeys Modifier { get; set; }

        public System.Windows.Forms.Keys Key { get; set; }

        public HotKey() { }

        public HotKey(Keys keys, string text, bool isMacro)
        {
            Modifier = keys.ConvertToModKeys();
            Key = keys.KeyWithoutMods();
            Text = text;
            IsMacro = isMacro;
        }

        public Keys ToKeysFormat()
        {
            return Key
                | (Modifier.HasFlag(ModKeys.Alt) ? Keys.Alt : 0)
                | (Modifier.HasFlag(ModKeys.Control) ? Keys.Control : 0)
                | (Modifier.HasFlag(ModKeys.Shift) ? Keys.Shift : 0)
                | (Modifier.HasFlag(ModKeys.Win) ? Keys.LWin : 0);
        }
    }

    public static class helper
    {
        public static ModKeys ConvertToModKeys(this Keys k)
        {
            return (k.HasFlag(Keys.Alt) ? ModKeys.Alt : 0)
                | (k.HasFlag(Keys.Control) ? ModKeys.Control : 0)
                | (k.HasFlag(Keys.Shift) ? ModKeys.Shift : 0)
                | (k.HasFlag(Keys.LWin) ? ModKeys.Win : 0);
        }

        public static Keys KeyWithoutMods(this Keys k)
        {
            Keys r = 0;
            foreach (var item in k.GetFlags().Except(HotKey.modifiers))
            {
                r |= item;
            }
            return r;
        }

        public static string ToStringExt(this Keys k)
        {
            return $"[{k.ConvertToModKeys()}] + {k.KeyWithoutMods()}";
        }

        public static IEnumerable<T> GetFlags<T>(this T input) where T : IConvertible
        {
            var e = input as Enum;
            return input?.ToString().Split(',').Select(x => (T)Enum.Parse(typeof(T), x.Trim()));
        }

    }
}
