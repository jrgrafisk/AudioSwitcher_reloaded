using System;
using System.Diagnostics;
using System.Windows.Forms;
using FortyOne.AudioSwitcher.AudioSwitcherService;
using FortyOne.AudioSwitcher.Helpers;

namespace FortyOne.AudioSwitcher
{
    public partial class UpdateForm : Form
    {
        private readonly string changelog = "";
        private readonly string url = "";

        public UpdateForm()
        {
            InitializeComponent();
            using (var client = ConnectionHelper.GetAudioSwitcherProxy())
            {
                url = client.CheckForUpdate(AudioSwitcher.Instance.AssemblyVersion) ?? "";
            }
        }

        public UpdateForm(AudioSwitcherVersionInfo vi)
        {
            InitializeComponent();
            url = vi.URL;
            changelog = vi.ChangeLog;
            toolTip1.SetToolTip(linkLabel1, url);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(url);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnUpdateNow_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(url))
                Process.Start(url);
            Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            var clf = new ChangeLogForm(changelog.Replace("\n", Environment.NewLine));
            clf.ShowDialog(this);
        }
    }
}