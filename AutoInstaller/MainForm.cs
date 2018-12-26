using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Threading;

namespace AutoInstaller
{
    public partial class MainForm : Form
    {
        string dataDirectory;
        HookStringWriter progressLog;

        public MainForm()
        {
            InitializeComponent();

            progressLog = new HookStringWriter(Console.Out);
            progressLog.TextChanged += ProgressLog_TextChanged;

            Console.SetOut(progressLog);

            if (AppSettings.Default.rememberPath && Directory.Exists(AppSettings.Default.dirPath))
            {
                saveCb.Checked = AppSettings.Default.rememberPath;
                dataDirectory = AppSettings.Default.dirPath;
                exePathTb.Text = AppSettings.Default.exePath;
                btnInstall.Enabled = true;
            }
        }

        private void ProgressLog_TextChanged(object sender, EventArgs e)
        {
            logTB.Text = progressLog.ToString();
        }

        public bool Install()
        {
            if (!Directory.Exists(dataDirectory + "\\Mods"))
            {
                Directory.CreateDirectory(dataDirectory + "\\Mods");
                Console.WriteLine("[Create folder] " + dataDirectory + "\\Mods");
            }

            progressBar.Value++;

            string managedPath = dataDirectory + "\\Managed\\";
            if (!Directory.Exists(managedPath))
            {
                Console.WriteLine("[ERROR] Managed folder does not exist");
                return false;
            }

            File.WriteAllBytes(managedPath + "ModLoader.dll", RequiredDlls.ModLoader);
            Console.WriteLine("[Copy] ModLoader.dll -> " + managedPath);
            progressBar.Value++;

            File.WriteAllBytes(managedPath + "0Harmony.dll", RequiredDlls._0Harmony);
            Console.WriteLine("[Copy] 0Harmony.dll -> " + managedPath);
            progressBar.Value++;

            string asmPath = managedPath + "Assembly-CSharp.dll";
            if (!File.Exists(asmPath))
            {
                Console.WriteLine("[ERROR] Assembly-CSharp.exe does not exist");
                return false;
            }

            Console.WriteLine("[Begin patch] Assembly-CSharp.dll");
            progressBar.Value++;
            Patcher.InjectMLLoader(asmPath, managedPath + "Assembly-CSharp.patched.dll");
            progressBar.Value++;
            Console.WriteLine("[End patch] Assembly-CSharp.patched.dll");
            progressBar.Value++;

            if (File.Exists(managedPath + "Assembly-CSharp.original.dll"))
            {
                Console.WriteLine("[Backup] File Assembly-CSharp.original.dll already exists");
            }
            else
            {
                File.Copy(managedPath + "Assembly-CSharp.dll", managedPath + "Assembly-CSharp.original.dll", true);
                Console.WriteLine("[Backup] Assembly-CSharp.dll -> Assembly-CSharp.original.dll");
            }
            
            progressBar.Value++;

            File.Copy(managedPath + "Assembly-CSharp.patched.dll", managedPath + "Assembly-CSharp.dll", true);
            Console.Write("[Replace] Assembly-CSharp.patched.dll -> Assembly-CSharp.dll");
            progressBar.Value++;

            return true;
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            progressBar.Minimum = 0;
            progressBar.Maximum = 8;
            progressBar.Value = 0;

            if (Install())
            {
                MessageBox.Show("ModLoader installed successfully!", "ModLoader Installer");
            }
            else
            {
                MessageBox.Show("An error occured during the installation. Please check the logs.",
                    "ModLoader Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar.Value = 0;
            }
        }

        private void selectBtn_Click(object sender, EventArgs e)
        {
            if (exeDialog.ShowDialog() == DialogResult.OK)
            {
                string gameDir = Path.GetDirectoryName(exeDialog.FileName);
                dataDirectory = Directory.GetDirectories(gameDir).FirstOrDefault(x => x.EndsWith("_Data"));
                if (dataDirectory == null || dataDirectory == "")
                {
                    MessageBox.Show("Could not find game Data directory", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnInstall.Enabled = false;
                    return;
                }

                exePathTb.Text = exeDialog.FileName;
                btnInstall.Enabled = true;

                AppSettings.Default.dirPath = dataDirectory;
                AppSettings.Default.exePath = exePathTb.Text;
                AppSettings.Default.Save();
            }
        }

        private void saveCb_CheckedChanged(object sender, EventArgs e)
        {
            AppSettings.Default.rememberPath = saveCb.Checked;
            AppSettings.Default.Save();
        }
    }
}
