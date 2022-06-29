/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 13 set 2019
 */

using i3EngineEditor.Managers;
using System.Text;

namespace i3EngineEditor.Tools
{
    public class Settings
    {
        static volatile Settings INSTANCE = new Settings();
        public static Settings gI() => INSTANCE;
        public bool EnableLog { get; set; } = true;
        public Encoding Encoding { get; set; }
        public Settings()
        {
        }
        public void Load()
        {
            SettingsManager sm = new SettingsManager("i3Settings.ini");
            EnableLog = bool.Parse(sm.Read("EnableLog"));
            Encoding = Encoding.GetEncoding(sm.Read("Encoding"));
        }
    }
}
