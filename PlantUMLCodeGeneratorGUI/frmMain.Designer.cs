namespace PlantUMLCodeGeneratorGUI
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.lstFolderList = new System.Windows.Forms.ListBox();
            this.btnAddFolder = new System.Windows.Forms.Button();
            this.btnRemoveFolder = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.chkShowProtectedMembers = new System.Windows.Forms.CheckBox();
            this.chkShowPrivateMembers = new System.Windows.Forms.CheckBox();
            this.chkShowOverriddenMembers = new System.Windows.Forms.CheckBox();
            this.grpSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstFolderList
            // 
            this.lstFolderList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFolderList.FormattingEnabled = true;
            this.lstFolderList.ItemHeight = 15;
            this.lstFolderList.Location = new System.Drawing.Point(13, 14);
            this.lstFolderList.Name = "lstFolderList";
            this.lstFolderList.Size = new System.Drawing.Size(214, 244);
            this.lstFolderList.TabIndex = 0;
            this.lstFolderList.SelectedIndexChanged += new System.EventHandler(this.lstFolderList_SelectedIndexChanged);
            // 
            // btnAddFolder
            // 
            this.btnAddFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnAddFolder.Location = new System.Drawing.Point(240, 14);
            this.btnAddFolder.Name = "btnAddFolder";
            this.btnAddFolder.Size = new System.Drawing.Size(87, 27);
            this.btnAddFolder.TabIndex = 1;
            this.btnAddFolder.Text = "Add";
            this.btnAddFolder.UseVisualStyleBackColor = true;
            this.btnAddFolder.Click += new System.EventHandler(this.btnAddFolder_Click);
            // 
            // btnRemoveFolder
            // 
            this.btnRemoveFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveFolder.Enabled = false;
            this.btnRemoveFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnRemoveFolder.Location = new System.Drawing.Point(240, 47);
            this.btnRemoveFolder.Name = "btnRemoveFolder";
            this.btnRemoveFolder.Size = new System.Drawing.Size(87, 27);
            this.btnRemoveFolder.TabIndex = 1;
            this.btnRemoveFolder.Text = "Remove";
            this.btnRemoveFolder.UseVisualStyleBackColor = true;
            this.btnRemoveFolder.Click += new System.EventHandler(this.btnRemoveFolder_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Enabled = false;
            this.btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnGenerate.Location = new System.Drawing.Point(240, 232);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(87, 27);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // grpSettings
            // 
            this.grpSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSettings.Controls.Add(this.chkShowProtectedMembers);
            this.grpSettings.Controls.Add(this.chkShowPrivateMembers);
            this.grpSettings.Controls.Add(this.chkShowOverriddenMembers);
            this.grpSettings.Location = new System.Drawing.Point(13, 265);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(312, 103);
            this.grpSettings.TabIndex = 2;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "Settings";
            // 
            // chkShowProtectedMembers
            // 
            this.chkShowProtectedMembers.AutoSize = true;
            this.chkShowProtectedMembers.Location = new System.Drawing.Point(7, 75);
            this.chkShowProtectedMembers.Name = "chkShowProtectedMembers";
            this.chkShowProtectedMembers.Size = new System.Drawing.Size(228, 19);
            this.chkShowProtectedMembers.TabIndex = 0;
            this.chkShowProtectedMembers.Text = "Include Protected Members && Methods";
            this.chkShowProtectedMembers.UseVisualStyleBackColor = true;
            // 
            // chkShowPrivateMembers
            // 
            this.chkShowPrivateMembers.AutoSize = true;
            this.chkShowPrivateMembers.Location = new System.Drawing.Point(7, 48);
            this.chkShowPrivateMembers.Name = "chkShowPrivateMembers";
            this.chkShowPrivateMembers.Size = new System.Drawing.Size(214, 19);
            this.chkShowPrivateMembers.TabIndex = 0;
            this.chkShowPrivateMembers.Text = "Include Private Members && Methods";
            this.chkShowPrivateMembers.UseVisualStyleBackColor = true;
            // 
            // chkShowOverriddenMembers
            // 
            this.chkShowOverriddenMembers.AutoSize = true;
            this.chkShowOverriddenMembers.Location = new System.Drawing.Point(7, 22);
            this.chkShowOverriddenMembers.Name = "chkShowOverriddenMembers";
            this.chkShowOverriddenMembers.Size = new System.Drawing.Size(168, 19);
            this.chkShowOverriddenMembers.TabIndex = 0;
            this.chkShowOverriddenMembers.Text = "Include Overriden Methods";
            this.chkShowOverriddenMembers.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 377);
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnRemoveFolder);
            this.Controls.Add(this.btnAddFolder);
            this.Controls.Add(this.lstFolderList);
            this.Font = new System.Drawing.Font("Segoe UI Semilight", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PlantUML Class Diagram Generater";
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstFolderList;
        private System.Windows.Forms.Button btnAddFolder;
        private System.Windows.Forms.Button btnRemoveFolder;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.CheckBox chkShowProtectedMembers;
        private System.Windows.Forms.CheckBox chkShowPrivateMembers;
        private System.Windows.Forms.CheckBox chkShowOverriddenMembers;
    }
}

