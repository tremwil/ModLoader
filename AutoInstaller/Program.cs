using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace AutoInstaller
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (e, s) =>
            {
                string shortName = s.Name.Substring(0, s.Name.IndexOf(",")).Replace('.', '_');
                object resx = RequiredDlls.ResourceManager.GetObject(shortName);
                return (resx == null) ? null : Assembly.Load((byte[])resx);
            };

            Application.EnableVisualStyles();
            Application.DoEvents();
            Application.Run(new MainForm());
        }
    }
}
