/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 11 jul 2019
 */

using i3EngineEditor.Packet;
using i3EngineEditor.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace i3EngineEditor.Managers
{
    public class TableManager
    {
        //public List<StringTable> _stringTables = new List<StringTable>();
        public List<int> tableKeys = new List<int>();
        public byte[] buffer;
        public bool Load(i3Editor form, Read r)
        {
            try
            {
                int Index = 0;
                buffer = r.ReadBytes((int)form.h.StringTableSizes);
                //using (MemoryStream stream = new MemoryStream(buffer))
                //using (Read reader = new Read(new BinaryReader(stream)))
                //{
                //    int Keep = 0, Count = 0;
                //    string TableName = "";
                //    foreach (string hex in BitConverter.ToString(buffer, 0).Split('-'))
                //    {
                //        if (Keep == 1 || Convert.ToInt32(hex, 16) < 16)
                //            Keep++;
                //        else Count++;
                //        if (Keep == 2)
                //        {
                //            Table t = new Table(reader.ReadString(Count));
                //            reader.ReadUShort(); //2573
                //            if (t.Name.Contains("i3"))
                //            {
                //                if (!tableKeys.ContainsKey(t.Name) && !t.Name.Equals(TableName))
                //                {
                //                    TableName = t.Name;
                //                    tableKeys.Add(t.Name, new List<Table>());
                //                }
                //            }
                //            else
                //            {
                //                t.Key = TableName;
                //                if (tableKeys.TryGetValue(TableName, out List<Table> value))
                //                    value.Add(t);
                //                else Logger.gI().Info($"TableKeys does not consist.", MessageBoxIcon.Warning);
                //            }
                //            Count = 0;
                //            Keep = 0;
                //            if (form.updateBar(Index++, form.h.StringTableCount))
                //                continue;
                //        }
                //    }
                //}
                //if (Index != form.h.StringTableCount)
                //    throw new Exception($"Tables index ({Index}!={form.h.StringTableCount}) error.");
            }
            catch (Exception ex)
            {
                form.setState(2);
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
                return false;
            }
            return true;
        }
    }
}