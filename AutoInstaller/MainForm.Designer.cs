namespace AutoInstaller
{
    partial class MainForm
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
            this.selectBtn = new System.Windows.Forms.Button();
            this.exePathTb = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.saveCb = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.logTB = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnInstall = new System.Windows.Forms.Button();
            this.exeDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // selectBtn
            // 
            this.selectBtn.Location = new System.Drawing.Point(6, 27);
            this.selectBtn.Name = "selectBtn";
            this.selectBtn.Size = new System.Drawing.Size(253, 29);
            this.selectBtn.TabIndex = 0;
            this.selectBtn.Text = "Select Unity Game Executable";
            this.selectBtn.UseVisualStyleBackColor = true;
            this.selectBtn.Click += new System.EventHandler(this.selectBtn_Click);
            // 
            // exePathTb
            // 
            this.exePathTb.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.exePathTb.Location = new System.Drawing.Point(6, 62);
            this.exePathTb.Name = "exePathTb";
            this.exePathTb.ReadOnly = true;
            this.exePathTb.Size = new System.Drawing.Size(478, 22);
            this.exePathTb.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.saveCb);
            this.groupBox1.Controls.Add(this.exePathTb);
            this.groupBox1.Controls.Add(this.selectBtn);
            this.groupBox1.Location = new System.Drawing.Point(13, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(490, 101);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // saveCb
            // 
            this.saveCb.AutoSize = true;
            this.saveCb.Location = new System.Drawing.Point(275, 30);
            this.saveCb.Name = "saveCb";
            this.saveCb.Size = new System.Drawing.Size(196, 21);
            this.saveCb.TabIndex = 2;
            this.saveCb.Text = "Save when patching again";
            this.saveCb.UseVisualStyleBackColor = true;
            this.saveCb.CheckedChanged += new System.EventHandler(this.saveCb_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.logTB);
            this.groupBox2.Controls.Add(this.progressBar);
            this.groupBox2.Location = new System.Drawing.Point(13, 158);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(490, 238);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Progress";
            // 
            // logTB
            // 
            this.logTB.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.logTB.Location = new System.Drawing.Point(6, 63);
            this.logTB.Multiline = true;
            this.logTB.Name = "logTB";
            this.logTB.ReadOnly = true;
            this.logTB.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logTB.Size = new System.Drawing.Size(478, 169);
            this.logTB.TabIndex = 1;
            this.logTB.WordWrap = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(6, 32);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(478, 25);
            this.progressBar.TabIndex = 0;
            // 
            // btnInstall
            // 
            this.btnInstall.Enabled = false;
            this.btnInstall.Location = new System.Drawing.Point(13, 119);
            this.btnInstall.MaximumSize = new System.Drawing.Size(490, 33);
            this.btnInstall.MinimumSize = new System.Drawing.Size(490, 33);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(490, 33);
            this.btnInstall.TabIndex = 4;
            this.btnInstall.Text = "Install Mod Loader";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // exeDialog
            // 
            this.exeDialog.DefaultExt = "exe";
            this.exeDialog.Filter = "EXE File|*.exe";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 408);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximumSize = new System.Drawing.Size(533, 455);
            this.MinimumSize = new System.Drawing.Size(533, 455);
            this.Name = "MainForm";
            this.Text = "Unity Mod Loader Installer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button selectBtn;
        private System.Windows.Forms.TextBox exePathTb;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox saveCb;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox logTB;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.OpenFileDialog exeDialog;
    }
}