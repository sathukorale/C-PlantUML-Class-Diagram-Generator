using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlantUMLCodeGeneratorGUI
{
    public partial class frmMain : Form
    {
        private string _lastVisitedDirectory = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = _lastVisitedDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                fbd.SelectedPath = @"D:\documents\marketdata\include\dissemination";
                fbd.Description = "Please select the folder which contains the header files";

                if (fbd.ShowDialog() == DialogResult.OK && (lstFolderList.Items.Cast<string>().Any(i => i == fbd.SelectedPath) == false))
                {
                    lstFolderList.Items.Add(fbd.SelectedPath);
                    _lastVisitedDirectory = fbd.SelectedPath;

                    btnGenerate.Enabled = true;
                }
            }
        }

        private void btnRemoveFolder_Click(object sender, EventArgs e)
        {
            if (lstFolderList.Items.Count > 0)
            {
                var selectedItems = lstFolderList.SelectedItems.OfType<object>().ToArray();
                for (var i = 0; i < selectedItems.Length; i++)
                {
                    lstFolderList.Items.Remove(selectedItems[i]);
                }
            }

            btnGenerate.Enabled = lstFolderList.Items.Count > 0;
        }

        private void lstFolderList_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemoveFolder.Enabled = lstFolderList.SelectedItems.Count > 0;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var generatedContent = frmLoadingDialog.ShowWindow(o =>
            {
                var headerFiles = lstFolderList.Items.Cast<string>().Select(i => Directory.GetFiles(i, "*.h", SearchOption.AllDirectories)).SelectMany(i => i);

                var completeContent = "";
                foreach (var file in headerFiles)
                {
                    completeContent += Environment.NewLine + File.ReadAllText(file);
                }

                var output = Processor.Process(completeContent, new Settings(chkShowOverriddenMembers.Checked, chkShowPrivateMembers.Checked, chkShowProtectedMembers.Checked));

                var content = "";
                content += "@startuml" + Environment.NewLine;
                content += "left to right direction" + Environment.NewLine;
                content += Environment.NewLine;
                content += output + Environment.NewLine;
                content += Environment.NewLine;
                content += "@enduml";

                return content;
            }, null, "Processing Files...", false) as string;

            using (var sfd = new SaveFileDialog())
            {
                sfd.OverwritePrompt = true;
                sfd.Filter = "WSD File|*.wsd";
                sfd.Title = "Save Generated PlantUML File";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                        File.Delete(sfd.FileName);

                    File.WriteAllText(sfd.FileName, generatedContent);
                }
            }

            MessageBox.Show("The PlantUML script was generated and saved successfully !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
