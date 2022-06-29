/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 15 jul 2019
 */

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace i3EngineEditor.Tools
{
    public class Write : IDisposable
    {
        public BinaryWriter write;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        public Write(BinaryWriter write)
        {
            this.write = write;
        }
        public void WriteByte(byte value)
        {
            write.Write(value);
        }
        public void WriteShort(short value)
        {
            write.Write(value);
        }
        public void WriteUShort(ushort value)
        {
            write.Write(value);
        }
        public void WriteInt(int value)
        {
            write.Write(value);
        }
        public void WriteUInt(uint value)
        {
            write.Write(value);
        }
        public void WriteFloat(float value)
        {
            write.Write(value);
        }
        public void WriteDouble(double value)
        {
            write.Write(value);
        }
        public void WriteLong(long value)
        {
            write.Write(value);
        }
        public void WriteULong(ulong value)
        {
            write.Write(value);
        }
        public void WriteString(string value, int count)
        {
            if (value != null)
            {
                WriteBytes(Settings.gI().Encoding.GetBytes(value));
                WriteBytes(new byte[count - value.Length]);
            }
        }
        public void WriteUString(string value)
        {
            if (value != null)
                WriteBytes(Encoding.Unicode.GetBytes(value));
        }
        public void WriteBytes(byte[] value)
        {
            WriteBytes(value, 0, value.Length);
        }
        public void WriteBytes(byte[] value, int offset, int length)
        {
            write.Write(value, offset, length);
        }
        public void Dispose()
        {
            if (write != null)
            {
                if (write.BaseStream != null)
                {
                    Utils.RemoveEvents(write.BaseStream);
                    write.BaseStream.Flush();
                    write.BaseStream.Close();
                    write.BaseStream.Dispose();
                }
                Utils.RemoveEvents(write.BaseStream);
                write.Close();
                write.Dispose();
                write = null;
                Utils.RemoveEvents(handle);
                handle.Close();
                handle.Dispose();
                handle = null;
                Utils.RemoveEvents(this);
            }
        }
    }
}