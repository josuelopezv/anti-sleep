using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using anti_sleep;
using API;

namespace WindowsFormsApplication1
{
    public partial class Form1 : HotKeyForm
    {
        private AntiSleep antiSleep = new AntiSleep();
        private static Settings settings = Settings.Load();

        public Form1()
        {
            InitializeComponent();
            chkAsEnabled.Checked = settings.asEnabled;
            if (settings.HotKeys?.Count() > 0)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Rows.Add(settings.HotKeys.Count());
                for (int i = 0; i < settings.HotKeys.Count(); i++)
                {
                    var hk = settings.HotKeys.ElementAt(i);
                    dataGridView1.Rows[i].Cells[0].Value = hk.ToKeysFormat().ToStringExt();
                    dataGridView1.Rows[i].Cells[0].Tag = hk.ToKeysFormat();
                    dataGridView1.Rows[i].Cells[1].Value = hk.Text;
                    dataGridView1.Rows[i].Cells[2].Value = hk.IsMacro;
                }
                RegisterAndReplace(settings.HotKeys);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void save()
        {
            var lhk = new List<HotKey>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Tag == null || row.Cells[0].ErrorText != string.Empty) continue;
                lhk.Add(new HotKey(((Keys)row.Cells[0].Tag), (string)row.Cells[1].Value, (bool)row.Cells[2]?.Value));
            }
            settings.HotKeys = lhk;
            settings.Save();
            RegisterAndReplace(settings.HotKeys);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            save();
            System.Environment.Exit(1);
        }

        private void chkAsEnabled_CheckedChanged(object sender, EventArgs e)
        {
            antiSleep.Enabled = settings.asEnabled = chkAsEnabled.Checked;
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (dataGridView1.IsCurrentRowDirty && e.Modifiers > 0 && (e.KeyData ^ e.Modifiers) > 0)
            {
                e.SuppressKeyPress = true;
                dataGridView1.CurrentRow.Cells[0].Value = e.KeyData.ToStringExt();
                dataGridView1.CurrentRow.Cells[0].Tag = e.KeyData;
            }
        }

        private void dataGridView1_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            var row = dataGridView1.Rows[e.RowIndex];
            if (row.Cells[0].Tag == null)
                row.Cells[0].ErrorText = "Required";
            else
                row.Cells[0].ErrorText = null;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys(v=vs.110).aspx");
        }
    }
}