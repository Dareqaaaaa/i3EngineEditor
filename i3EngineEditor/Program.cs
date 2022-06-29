/* TeamExploit \o/
 * 
 * Build by PISTOLA
 * 
 * 11 jul 2019
 */

using System;
using System.Windows.Forms;

namespace i3EngineEditor
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new i3Editor(args));
        }
    }
}