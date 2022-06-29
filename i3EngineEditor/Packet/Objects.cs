/* TeamExploit \o/
 *
 * File created by PISTOLA
 * 
 * 11 jul 2019
 */

using i3EngineEditor.Tools;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace i3EngineEditor.Packet
{
    public class Objects : IDisposable
    {
        public int Type { get; set; }
        public ulong ID { get; set; }
        public ulong Offset { get; set; }
        public ulong Size { get; set; }
        public int Initial { get; set; }
        public ulong Children { get; set; }
        public byte[] Buffer { get; set; }
        public StateObject State { get; set; } = new StateObject();
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        public Objects(ulong ID)
        {
            this.ID = ID;
        }
        public Objects(int Type, ulong ID, ulong Offset, ulong Size)
        {
            this.Type = Type;
            this.ID = ID;
            this.Offset = Offset;
            this.Size = Size;          
        }
        public class StateObject
        {
            public string Name { get; set; }
            public int ItemType { get; set; }
            public ValueEnum ValueType { get; set; }
            public int Nations { get; set; }
            public Dictionary<ulong, Objects> Items { get; set; } = new Dictionary<ulong, Objects>();
            public List<object> ItemsNations { get; set; } = new List<object>();
            public List<object> List1 { get; set; } = new List<object>();
            public List<object> List2 { get; set; } = new List<object>();
            public ListView ListView { get; set; } = new ListView();
        }
        public void Dispose()
        {
            State.Items.Clear();
            State.Items = null;
            State.ItemsNations.Clear();
            State.ItemsNations = null;
            State.List1.Clear();
            State.List1 = null;
            State.List2.Clear();
            State.List2 = null;
            State.ListView.Clear();
            State.ListView.Dispose();
            State.ListView = null;
            Utils.RemoveEvents(State);
            GC.ReRegisterForFinalize(State);
            State = null;
            Utils.RemoveEvents(handle);
            handle.Close();
            handle.Dispose();
            handle = null;
        }
    }
}