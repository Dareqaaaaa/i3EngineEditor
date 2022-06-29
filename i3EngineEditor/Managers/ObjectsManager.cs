/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 11 jul 2019
 */

using i3EngineEditor.Packet;
using i3EngineEditor.Tools;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace i3EngineEditor.Managers
{
    public class ObjectsManager
    {
        public Dictionary<ulong, Objects> objectKeys = new Dictionary<ulong, Objects>();
        public List<Objects> all = new List<Objects>();
        public bool Load(i3Editor form, Read reader)
        {
            try
            {
                lock (objectKeys)
                {
                    form.setState(1);
                    form.updateBar(-1, 0);
                    reader.reader.BaseStream.Position = (long)form.h.ObjectInfoOffset;
                    for (int i = 0; i < form.h.ObjectInfoCount; i++)
                    {
                        Objects obj = new Objects(0)
                        {
                            Type = reader.ReadInt(),
                            ID = reader.ReadULong(),
                            Offset = reader.ReadULong(),
                            Size = reader.ReadULong()
                        };
                        objectKeys.Add(obj.ID, obj);
                        if (form.updateBar((int)obj.ID, form.h.ObjectInfoCount))
                            continue;
                    }
                    form.updateBar(-1, 0);
                    foreach (Objects obj in objectKeys.Values)
                    {
                        if (form.fext == FilesExt.PEF)
                        {
                            reader.reader.BaseStream.Position = (long)obj.Offset;
                            obj.State.Name = reader.ReadString(reader.ReadByte());
                            //obj.State.Name = reader.ReadString(reader.ReadByte(), true);
                            string TRN = reader.ReadString(4);
                            if ((TRN.Equals("TRN3") || (TRN.Contains("RN3") || TRN.Contains("N3")) && obj.Type > 0) || obj.ID == (ulong)objectKeys.Count)
                            {
                                obj.Initial = 1;
                                reader.ReadShort();
                                obj.Initial += reader.ReadInt();
                                reader.ReadShort();
                                int items = reader.ReadInt();
                                reader.ReadBytes(60);
                                for (int i = 0; i < items; i++)
                                {
                                    ulong id = (ulong)reader.ReadInt();
                                    if (objectKeys.TryGetValue(id, out Objects o))
                                    {
                                        obj.State.Items.Add(o.ID, o);
                                        obj.State.List1.Add(o.ID);
                                    }
                                    else throw new Exception($"ID {id} failed to TryGetValue1.");
                                }
                                reader.ReadString(4); //RGK1
                                items = reader.ReadInt();
                                reader.ReadULong();
                                for (int i = 0; i < items; i++)
                                {
                                    ulong id = 0;
                                    id = reader.ReadULong();
                                    if (objectKeys.TryGetValue(id, out Objects o))
                                    {
                                        obj.State.Items.Add(o.ID, o);
                                        obj.State.List2.Add(o.ID);
                                        o.Children = obj.ID;
                                    }
                                    else throw new Exception($"ID {id} failed to TryGetValue2.");
                                }
                            }
                            else
                            {
                                reader.reader.BaseStream.Position -= 4; //TRN3
                                obj.State.ItemType = reader.ReadInt();
                                obj.State.ValueType = (ValueEnum)(obj.State.ItemType == 9 ? reader.ReadInt() : obj.State.ItemType);
                                obj.State.Nations = obj.State.ItemType == 9 ? reader.ReadInt() : 1;
                                //if (obj.State.Nations > 1 && obj.State.Nations > form.clients.Count)
                                //    throw new Exception("Oops! The Nation.nlf file may not be compatible with this file. " + obj.State.Nations);
                                for (int i = 0; i < obj.State.Nations; i++)
                                {
                                    switch (obj.State.ValueType)
                                    {
                                        case ValueEnum.INT32:
                                            {
                                                obj.State.ItemsNations.Add(reader.ReadInt());
                                                break;
                                            }
                                        case ValueEnum.FLOAT:
                                            {
                                                obj.State.ItemsNations.Add(reader.ReadFloat());
                                                break;
                                            }
                                        case ValueEnum.STRING:
                                            {
                                                reader.ReadString(4); //RGS3
                                                obj.State.ItemsNations.Add(reader.ReadUString((int)reader.ReadUInt() * 2));
                                                break;
                                            }
                                        case ValueEnum.POS1:
                                            {
                                                Half3 half = new Half3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
                                                obj.State.ItemsNations.Add($"X: {half.X}; Y: {half.Y}; Z: {half.Z}");
                                                break;
                                            }
                                        case ValueEnum.POS2:
                                            {
                                                Half4 half = new Half4(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
                                                obj.State.ItemsNations.Add($"X: {half.X}; Y: {half.Y}; Z: {half.Z}; W: {half.W}");
                                                break;
                                            }
                                        case ValueEnum.HEX:
                                            {
                                                obj.State.ItemsNations.Add(reader.ReadHString(4));
                                                break;
                                            }
                                        default:
                                            {
                                                Logger.gI().Info($"Object: '{obj.State.Name}' has unknown value type '{obj.State.ValueType}'; IndexOf: {obj.Offset}", MessageBoxIcon.Warning);
                                                break;
                                            }
                                    }
                                }
                            }
                            //if (reader.ReadableBytes() > 0)
                            //    throw new Exception($"[Object: {obj.State.Name}; IndexOf: {obj.Offset}; Length: {reader.Length}; Size: {obj.Size}; Nations: {obj.State.ItemsNations.Count}/{obj.State.Nations}] {Environment.NewLine}Buffer integrity error {reader.ReadableBytes()}.");
                        }
                        else if (form.fext == FilesExt.I3GAME)
                        {
                            obj.State.Name = reader.ReadString(reader.ReadByte());
                            if (reader.ReadString(4).Equals("STG1"))
                            {
                                obj.Initial = 1;
                                string named = reader.ReadString(67);
                                reader.ReadBytes(25);
                                int items = reader.ReadInt();
                                reader.ReadBytes(60);
                                for (int i = 0; i < items; i++)
                                {
                                    if (objectKeys.TryGetValue(reader.ReadULong(), out Objects o))
                                    {
                                        obj.State.Items.Add(o.ID, o);
                                        obj.State.List1.Add(o.ID);
                                        o.Children = obj.ID;
                                    }
                                }
                            }
                            else if (reader.ReadString(4).Equals("GND1"))
                            {
                                obj.Initial = 2;
                                reader.ReadBytes(28);
                                reader.ReadString(4); //FRW1
                                reader.ReadInt();
                                int items = reader.ReadInt();
                                reader.ReadBytes(60);
                                for (int i = 0; i < items; i++)
                                {
                                    if (objectKeys.TryGetValue(reader.ReadULong(), out Objects o))
                                    {
                                        obj.State.Items.Add(o.ID, o);
                                        obj.State.List1.Add(o.ID);
                                        o.Children = obj.ID;
                                    }
                                }
                            }
                            else
                            {
                                reader.reader.BaseStream.Position -= 4; //STG1
                                reader.ReadShort();
                                byte value = reader.ReadByte();
                                if (value == 1)
                                    reader.ReadBytes(33);
                                else
                                    reader.ReadBytes(29);
                            }
                            if (reader.ReadableBytes() > 0)
                                throw new Exception($"[Object: {obj.State.Name}; IndexOf: {obj.Offset}] Buffer integrity error {reader.ReadableBytes()}.");
                        }
                        else if (form.fext == FilesExt.I3PACK)
                        {
                            obj.State.Name = reader.ReadString(reader.ReadByte());
                            if (reader.ReadString(4).Equals("TRN3"))
                            {

                                //byte enc = reader.ReadByte();
                                //reader.reader.BaseStream.Position--;
                                //byte[] data = reader.ReadBytes(reader.ReadableBytes());
                                //if (enc < 64) Utils.Dec(data, obj.ID == 1 ? 2 : 3, -1);
                                //using (MemoryStream mstream = new MemoryStream(data))
                                //using (Read breader = new Read(new BinaryReader(mstream), encrypted))
                                //{
                                //    int lengthName = breader.ReadInt();                        
                                //    breader.ReadBytes(lengthName);
                                //    breader.ReadInt();
                                //    breader.ReadLong(); //?
                                //    long files = ((long)obj.Size - breader.Position()) / 88;
                                //    for (int i = 0; i < files; i++)
                                //    {
                                //        Objects item = new Objects((ulong)i + 1);
                                //        item.State.Name = breader.ReadString(lengthName).Trim();
                                //        breader.ReadUShort(); //0
                                //        //breader.ReadUInt(); //?

                                //        byte sizeor1 = breader.ReadByte();
                                //        byte offset1 = breader.ReadByte();
                                //        byte size1 = breader.ReadByte();
                                //        breader.ReadUInt();
                                //        byte offsetor1 = breader.ReadByte();
                                //        breader.ReadUInt();
                                //        byte sizeor2 = breader.ReadByte();
                                //        byte offset2 = breader.ReadByte();
                                //        byte size2 = breader.ReadByte();
                                //        breader.ReadUInt();
                                //        byte offsetor2 = breader.ReadByte();
                                //        breader.ReadBytes(3);
                                //        uint ended = breader.ReadUInt();

                                //        /*

                                //        Offset: 8F7DC Size: 5F84A

                                //        Name -> 41 49 5F 4D 61 64 6E 65 73 73 2E 70 65 66 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
                                //        00 00 00 00 

                                //        sizeor1 >20 
                                //        31 4A F8 08 00 05 00 00 00 09 59 DC F7 00 00 00 00 00 00 00 00 00 00



                                //        //breader.ReadUShort();
                                //        //breader.ReadULong(); //0

                                //        /*
                                //         char szFilename[32]; //0x0000 
                                //         char _0x0020[20];
                                //         char N0193C181[2]; //0x0034 
                                //         WORD SizeOr_1; //0x0036 52
                                //         WORD OffsShift_1; //0x0038 54
                                //         WORD SizeShift_1; //0x003A 56
                                //         char _0x003C[4];
                                //         WORD OffsOr_1; //0x0040 5c
                                //         char N01970D81[4]; //0x0042 
                                //         WORD SizeOr_2; //0x0046 52
                                //         WORD OffsShift_2; //0x0048 54
                                //         WORD SizeShift_2; //0x004A 56

                                //         char N01A26723[4]; //0x004C 
                                //         WORD OffsOr_2; //0x0050 5c
                                //         char N019E3220[3]; //0x0052 
                                //         DWORD Ended; //0x0055*/


                                //        string ioffset = "", isize = "";
                                //        for (int j = 0; j < 2; j++)
                                //        {
                                //            breader.ReadUInt(); //?
                                //            byte[] cache = breader.ReadBytes(2);
                                //            Array.Reverse(cache);
                                //            string edata = breader.ReadHString(cache);
                                //            if (j == 0)
                                //            {
                                //                cache = breader.ReadBytes(2);
                                //                Array.Reverse(cache);
                                //                ioffset = breader.ReadHString(cache);
                                //                cache = breader.ReadBytes(2);
                                //                Array.Reverse(cache);
                                //                isize = breader.ReadHString(cache);
                                //                item.Size = ulong.Parse(isize + edata, NumberStyles.HexNumber);
                                //            }
                                //            else
                                //            {
                                //                item.Offset = ulong.Parse(ioffset + edata, NumberStyles.HexNumber);
                                //            }
                                //        }
                                //        form.updateBar(i + 1, (int)files);


                                //        string path = $"{Directory.GetCurrentDirectory()}//Dump";
                                //        if (!Directory.Exists(path))
                                //            Directory.CreateDirectory(path);

                                //        using (MemoryStream fstream = new MemoryStream(form.fileBuffer))
                                //        using (Read freader = new Read(new BinaryReader(fstream), encrypted))
                                //        {
                                //            freader.reader.BaseStream.Position = (long)item.Offset;
                                //            item.Buffer = freader.ReadBytes((int)item.Size);

                                //            using (FileStream ff = new FileStream("Dump//" + item.State.Name, FileMode.Create))
                                //            using (BinaryWriter ww = new BinaryWriter(ff))
                                //            {
                                //                ww.Write(item.Buffer);
                                //            }
                                //        }
                                //        obj.State.Items.Add(item.ID, item);
                                //        //MessageBox.Show($"Name: {item.State.Name}; Size: {item.Size}; Offset: {item.Offset}; Buffer: {item.Buffer.Length}");
                                //    }
                                //    if (reader.ReadableBytes() > 0 || breader.ReadableBytes() > 0) throw new Exception($"[Object: {obj.State.Name}; IndexOf: {obj.Offset}] Buffer integrity error.");
                                //}
                            }
                        }
                        if (form.updateBar((int)obj.ID, form.h.ObjectInfoCount))
                            continue;
                    }
                    //if (((objectKeys.Count * 28) == (int)form.h.ObjectInfoSize) && r.Position() == (long)form.h.ItemsOffset)
                    //{

                    //    if (r.ReadableBytes() > 0) throw new Exception($"File buffer integrity error.");
                    //}
                    //else throw new Exception($"Objects offset ({r.Position()}!={form.h.ItemsOffset}) error.");
                }
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
        public Objects registryRoot()
        {
            foreach (Objects obj in objectKeys.Values)
                if (obj != null && obj.Initial > 1)
                    return obj;
            return null;
        }
    }
}