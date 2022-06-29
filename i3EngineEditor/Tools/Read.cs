/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 11 jul 2019
 */

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace i3EngineEditor.Tools
{
    public class Read : IDisposable
    {
        public BinaryReader reader;
        public long Length;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        public Read(BinaryReader reader)
        {
            this.reader = reader;
            Length = reader.BaseStream.Length;
        }
        public int ReadableBytes() => (int)(Length - reader.BaseStream.Position);
        public long Position() =>  reader.BaseStream.Position;
        public byte[] ReadAllBytes() => reader.ReadBytes(ReadableBytes());
        public byte[] ReadBytes(int count) => reader.ReadBytes(count);
        public byte ReadByte() => reader.ReadByte();
        public short ReadShort() => reader.ReadInt16();
        public ushort ReadUShort() => reader.ReadUInt16();
        public int ReadInt() => reader.ReadInt32();
        public uint ReadUInt() => reader.ReadUInt32();
        public long ReadLong() => reader.ReadInt64();
        public ulong ReadULong() => reader.ReadUInt64();
        public float ReadFloat() => reader.ReadSingle();
        public sbyte ReadSBytes() => reader.ReadSByte(); 
        public string ReadString(int qty) => ReadString(ReadBytes(qty));
        public string ReadUString(int qty) => ReadUString(ReadBytes(qty));
        public string ReadHString(int qty) => ReadHString(ReadBytes(qty));
        public string ReadHString(byte[] buffer) => BitConverter.ToString(buffer, 0, buffer.Length).Replace("-", "");
        public string ReadString(byte[] buffer)
        {
            string texto = Settings.gI().Encoding.GetString(buffer);
            int length = texto.IndexOf(char.MinValue);
            if (length != -1)
                texto = texto.Substring(0, length);
            return texto;
        }
        public string ReadUString(byte[] buffer)
        {
            string text = Settings.gI().Encoding.GetString(buffer);
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
                if (i % 2 == 0)
                    s.Append(text[i]);
            string txt = s.ToString();
            s = null;
            return txt;
        }
        public void Dispose()
        {
            if (reader != null)
            {
                if (reader.BaseStream != null)
                {
                    Utils.RemoveEvents(reader.BaseStream);
                    reader.BaseStream.Flush();
                    reader.BaseStream.Close();
                    reader.BaseStream.Dispose();
                }
                Utils.RemoveEvents(reader.BaseStream);
                reader.Close();
                reader.Dispose();
                reader = null;
                Utils.RemoveEvents(handle);
                handle.Close();
                handle.Dispose();
                handle = null;
                Utils.RemoveEvents(this);
            }
        }
    }
}