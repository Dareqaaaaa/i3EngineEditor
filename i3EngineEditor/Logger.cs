/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 11 jul 2019
 */

using i3EngineEditor.Tools;
using System;
using System.IO;
using System.Windows.Forms;

namespace i3EngineEditor
{
    public class Logger
    {
        static readonly Logger INSTANCE = new Logger();
        string date = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");
        public Logger()
        {
            CheckDirectorys();
        }
        public static Logger gI() => INSTANCE;
        public DialogResult Info(string text, MessageBoxIcon i)
        {
            DialogResult dialogResult = DialogResult.Cancel;
            try
            {
                Log(text);
                dialogResult = MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, i);
            }
            catch { }
            return dialogResult;
        }
        public void Log(string text)
        {
            try
            {
                Log(date, text);
            }
            catch { }
        }
        public void Log(string file, string text)
        {
            if (!Settings.gI().EnableLog)
                return;
            try
            {
                using (FileStream fileStream = new FileStream($"Logs/{file}.log", FileMode.Append))
                using (StreamWriter stream = new StreamWriter(fileStream))
                {
                    try
                    {
                        if (stream != null) stream.WriteLine(text);
                    }
                    catch { }
                    stream.Flush();
                    stream.Close();
                    fileStream.Flush();
                    fileStream.Close();
                    Utils.RemoveEvents(stream);
                    Utils.RemoveEvents(fileStream);
                }
            }
            catch { }
        }
        public void CheckDirectorys()
        {
            if (!Settings.gI().EnableLog)
                return;
            try
            {
                string path = $"{Directory.GetCurrentDirectory()}//Logs";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch { }
        }
    }
}