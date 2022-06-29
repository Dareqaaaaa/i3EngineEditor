/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 13 set 2019
 */

using i3EngineEditor.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace i3EngineEditor.Managers
{
    public class SettingsManager
    {
        public FileInfo File;
        public SortedList<string, string> Tables;
        public SettingsManager(string path)
        {
            try
            {
                File = new FileInfo(path);
                Tables = new SortedList<string, string>();
                Exception ex = null;
                if (!Load(ref ex))
                    throw ex;
            }
            catch (Exception ex)
            {
                Logger.gI().Info($"There was a problem reading settings.", MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
        }
        public bool Load(ref Exception ex)
        {
            try
            {
                using (StreamReader reader = new StreamReader(File.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();
                        if (line.Length != 0 && !line.StartsWith(";") && !line.StartsWith("[") && !line.StartsWith("#"))
                        {
                            string[] split = line.Split('=');
                            Tables.Add(split[0].Trim(), split[1].Trim());
                        }
                    }
                    reader.Close();
                    Utils.RemoveEvents(reader);
                }
                return true;
            }
            catch (Exception exception)
            {
                ex = exception;
                return false;
            }
        }
        public string Read(string parameter)
        {
            return Tables[parameter];
        }
    }
}