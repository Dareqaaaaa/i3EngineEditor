/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 26 jul 2019
 */

using i3EngineEditor.Tools;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace i3EngineEditor.Packet
{
    public class Header
    {
        public FileInfo file { get; set; }
        public StringBuilder builder { get; set; }
        public string Item { get; set; }
        public bool CryptDefault { get; set; }
        public ushort VersionMajor { get; set; }
        public ushort VersionMinor { get; set; }
        public int StringTableCount { get; set; }
        public ulong StringTableOffset { get; set; }
        public ulong StringTableSizes { get; set; }
        public int ObjectInfoCount { get; set; }
        public ulong ObjectInfoOffset { get; set; }
        public ulong ObjectInfoSize { get; set; }
        public int TreeNodeCount { get; set; }
        public int LengthBuffer { get; set; }
        public ulong ItemsOffset { get; set; }
        public int HeaderOffset { get; set; }
        public DDS dds { get; set; }
        public TGA tga { get; set; }
        public ushort Unknown { get; set; }
        public ImageFormat Format { get; set; } = ImageFormat.NULL;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        public Header(FileInfo file)
        {
            this.file = file;
        }
        public bool LoadI3R2(i3Editor form, byte[] buffer, long head, bool enc)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                using (Read r = new Read(new BinaryReader(stream)))
                {
                    builder = new StringBuilder();
                    builder.AppendLine($"Encoding: {Settings.gI().Encoding.EncodingName} ({Settings.gI().Encoding.HeaderName})");
                    builder.AppendLine($"File Name: {file.Name}");
                    builder.AppendLine($"File Length: {Utils.SizeSuffix(file.Length)}");
                    builder.AppendLine($"File Crypt: {CryptDefault = enc}");
                    builder.AppendLine($"CRC32: {Convert.ToString(Utils.CRC32(form.fileBuffer), 16).ToUpper()}");
                    builder.AppendLine($"Length: {(head == -1 ? r.reader.BaseStream.Length : head)}");
                    if (head != -1)
                        return true;
                    builder.AppendLine($"Item: {Item = r.ReadString(8).Trim()}");
                    if (Item.Equals("I3R2"))
                    {
                        builder.AppendLine($"VersionMajor: {VersionMajor = r.ReadUShort()}");
                        builder.AppendLine($"VersionMinor: {VersionMinor = r.ReadUShort()}");
                        builder.AppendLine($"StringTableCount: {StringTableCount = r.ReadInt()}");
                        builder.AppendLine($"StringTableOffset: {StringTableOffset = r.ReadULong()}");
                        builder.AppendLine($"StringTableSizes: {StringTableSizes = r.ReadULong()}");
                        builder.AppendLine($"ObjectInfoCount: {ObjectInfoCount = r.ReadInt()}");
                        builder.AppendLine($"ObjectInfoOffset: {ObjectInfoOffset = r.ReadULong()}");
                        builder.AppendLine($"ObjectInfoSize: {ObjectInfoSize = r.ReadULong()}");
                        builder.AppendLine($"TreeNodeCount: {TreeNodeCount = r.ReadInt()}");
                        r.ReadBytes(128);
                        builder.AppendLine($"ItemsOffset: {ItemsOffset = ((ulong)(HeaderOffset = (int)r.Position()) + StringTableSizes + ObjectInfoSize)}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
                
            }
            return false;
        }
        public bool LoadI3IB(i3Editor form, Read r, bool enc)
        {
            try
            {
                builder = new StringBuilder();
                builder.AppendLine($"Encoding: {Settings.gI().Encoding.EncodingName} ({Settings.gI().Encoding.HeaderName})");
                builder.AppendLine($"File Name: {file.Name}");
                builder.AppendLine($"File Length: {Utils.SizeSuffix(file.Length)}");
                builder.AppendLine($"File Crypt: {CryptDefault = enc}");
                builder.AppendLine($"CRC32: {Convert.ToString(Utils.CRC32(form.fileBuffer), 16).ToUpper()}");
                builder.AppendLine($"Length: {r.reader.BaseStream.Length}");
                builder.AppendLine($"Item: {Item = r.ReadString(4).Trim()}");
                if (Item.Equals("I3IB"))
                {
                    builder.AppendLine($"Unknown: {Unknown = r.ReadUShort()}");
                    ushort Height = r.ReadUShort();
                    ushort Width = r.ReadUShort();
                    uint Surface = r.ReadUInt();
                    r.ReadBytes(10);
                    ushort MipMapCount = r.ReadUShort();
                    ushort MarkerLen = r.ReadUShort();
                    r.ReadBytes(32);
                    if (r.Position() == 60)
                    {
                        switch (Surface)
                        {
                            case 0x31545844:
                            case 0xA0000681:
                            case 0x80000680: //DXT1
                            case 0x681:
                            case 0x680:
                                {
                                    Format = ImageFormat.DDS;
                                    dds = new DDS() { Header = 0x20534444, Size = 0x7C, Caps1 = DDS.DDSD_PIXELFORMAT, Flags = 0x81007, Depth = 1, Height = Height, Width = Width, Format = DDS.PixelFormat.DXT1, MipMapCount = MipMapCount, Surface = Surface };
                                    dds.PitchOrLinearSize = 0x2000;
                                    dds.StructPixelFormat.FourCC = DDS.FOURCC_DXT1;
                                    dds.StructPixelFormat.Size = 0x20;
                                    dds.StructPixelFormat.Flags = DDS.DDPF_FOURCC;
                                    dds.Data = r.ReadAllBytes();
                                    break;
                                }
                            case 0xA0000601:
                            case 0x32545844: //DXT2
                            case 0x601:
                                {
                                    Format = ImageFormat.DDS;
                                    dds = new DDS() { Header = 0x20534444, Size = 0x7C, Caps1 = DDS.DDSD_PIXELFORMAT, Flags = 0x81007, Depth = 1, Height = Height, Width = Width, Format = DDS.PixelFormat.DXT2, MipMapCount = MipMapCount, Surface = Surface };
                                    dds.PitchOrLinearSize = 0x4000;
                                    dds.StructPixelFormat.FourCC = DDS.FOURCC_DXT2;
                                    dds.StructPixelFormat.Size = 0x20;
                                    dds.StructPixelFormat.Flags = DDS.DDPF_FOURCC;
                                    dds.Data = r.ReadAllBytes();
                                    break;
                                }
                            case 0xA0000602:
                            case 0x33545844: //DXT3
                            case 0x602:
                                {
                                    Format = ImageFormat.DDS;
                                    dds = new DDS() { Header = 0x20534444, Size = 0x7C, Caps1 = DDS.DDSD_PIXELFORMAT, Flags = 0x81007, Depth = 1, Height = Height, Width = Width, Format = DDS.PixelFormat.DXT3, MipMapCount = MipMapCount, Surface = Surface };
                                    dds.PitchOrLinearSize = 0x6000;
                                    dds.StructPixelFormat.FourCC = DDS.FOURCC_DXT3;
                                    dds.StructPixelFormat.Size = 0x20;
                                    dds.StructPixelFormat.Flags = DDS.DDPF_FOURCC;
                                    dds.Data = r.ReadAllBytes();
                                    break;
                                }
                            case 0xA0000603:
                            case 0x34545844: //DXT4
                            case 0x603:
                                {
                                    Format = ImageFormat.DDS;
                                    dds = new DDS() { Header = 0x20534444, Size = 0x7C, Caps1 = DDS.DDSD_PIXELFORMAT, Flags = 0x81007, Depth = 1, Height = Height, Width = Width, Format = DDS.PixelFormat.DXT4, MipMapCount = MipMapCount, Surface = Surface };
                                    dds.PitchOrLinearSize = 0x8000;
                                    dds.StructPixelFormat.FourCC = DDS.FOURCC_DXT4;
                                    dds.StructPixelFormat.Size = 0x20;
                                    dds.StructPixelFormat.Flags = DDS.DDPF_FOURCC;
                                    dds.Data = r.ReadAllBytes();
                                    break;
                                }
                            case 0xA0000604:
                            case 0x35545844: //DXT5
                            case 0x604:
                                {
                                    Format = ImageFormat.DDS;
                                    dds = new DDS() { Header = 0x20534444, Size = 0x7C, Caps1 = DDS.DDSD_PIXELFORMAT, Flags = 0x81007, Depth = 1, Height = Height, Width = Width, Format = DDS.PixelFormat.DXT5, MipMapCount = MipMapCount, Surface = Surface };
                                    dds.PitchOrLinearSize = 0x10000;
                                    dds.StructPixelFormat.FourCC = DDS.FOURCC_DXT5;
                                    dds.StructPixelFormat.Size = 0x20;
                                    dds.StructPixelFormat.Flags = DDS.DDPF_FOURCC;
                                    dds.Data = r.ReadAllBytes();
                                    break;
                                }
                            case 0x20000406:
                                {
                                    Format = ImageFormat.TGA;
                                    tga = new TGA() { MarkerLen = MarkerLen };
                                    if (tga.MarkerLen > 0)
                                        tga.PathImage = r.ReadString((int)tga.MarkerLen);
                                    tga.Header = new TargaHeader();
                                    tga.Header.SetImageType(ImageType.UNCOMPRESSED_TRUE_COLOR);
                                    tga.Header.SetHeight((short)Height);
                                    tga.Header.SetWidth((short)Width);
                                    tga.Header.SetPixelDepth(0x10);
                                    tga.Data = r.ReadAllBytes();
                                    break;
                                }
                            default:
                                {
                                    throw new Exception($"Unrecognized format 0x{Convert.ToString(Surface, 16)}");
                                }
                        }
                        builder.AppendLine($"Height: {Height}px");
                        builder.AppendLine($"Width: {Width}px");
                        builder.AppendLine($"Format: {Format}");
                        if (dds != null)
                            builder.AppendLine($"PixelFormat: {dds.Format}");
                        builder.AppendLine($"MipMap: {MipMapCount}");
                        if (tga != null && !string.IsNullOrEmpty(tga.PathImage))
                            builder.AppendLine($"PathImage: {tga.PathImage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
                return false;
            }
            return r.Position() == r.Length;
        }
        public bool LoadVTIH(i3Editor form, Read r, bool enc)
        {
            try
            {
                builder = new StringBuilder();
                builder.AppendLine($"Encoding: {Settings.gI().Encoding.EncodingName} ({Settings.gI().Encoding.HeaderName})");
                builder.AppendLine($"File Name: {file.Name}");
                builder.AppendLine($"File Length: {Utils.SizeSuffix(file.Length)}");
                builder.AppendLine($"File Crypt: {CryptDefault = enc}");
                builder.AppendLine($"CRC32: {Convert.ToString(Utils.CRC32(form.fileBuffer), 16).ToUpper()}");
                builder.AppendLine($"Length: {r.reader.BaseStream.Length}");
                builder.AppendLine($"Item: {Item = r.ReadString(4).Trim()}");
                if (Item.Equals("VTIH"))
                {
                    builder.AppendLine($"Unknown: {Unknown = (ushort)r.ReadUInt()}");
                    ushort Height = r.ReadUShort();
                    r.ReadUShort();
                    ushort Width = r.ReadUShort();
                    r.ReadBytes(6);
                    uint MarkerLen = r.ReadUInt();
                    ushort MipMapCount = r.ReadUShort();
                    r.ReadBytes(102);
                    if (r.Position() == 128)
                    {
                        if (Unknown == 1)
                        {
                            Format = ImageFormat.TGA;
                            tga = new TGA() { MarkerLen = MarkerLen };
                            if (tga.MarkerLen > 0)
                                tga.PathImage = r.ReadString((int)tga.MarkerLen);
                            r.ReadBytes(1880);
                            tga.Header = new TargaHeader();
                            tga.Header.SetImageType(ImageType.UNCOMPRESSED_TRUE_COLOR);
                            tga.Header.SetHeight((short)Height);
                            tga.Header.SetWidth((short)Width);
                            tga.Header.SetPixelDepth(16);
                            //tga.Header.SetColorMapType(ColorMapType.NO_COLOR_MAP);
                            tga.Data = r.ReadAllBytes();
                        }
                        if (tga != null)
                        {
                            builder.AppendLine($"Height: {Height}px");
                            builder.AppendLine($"Width: {Width}px");
                            builder.AppendLine($"Format: {Format}");
                            builder.AppendLine($"MipMap: {MipMapCount}");
                            builder.AppendLine($"PathImage: {tga.PathImage}");
                        }
                        else
                        {
                            throw new Exception("Unrecognized format " + Convert.ToString(Unknown, 16));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
                return false;
            }
            return r.Position() == r.Length;
        }
        public bool ProcessImage(i3Editor form, Read r)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                using (Write writer = new Write(new BinaryWriter(stream)))
                {
                    if (tga != null && Format == ImageFormat.TGA)
                    {
                        writer.WriteByte(tga.Header.ImageIDLength);
                        writer.WriteByte((byte)tga.Header.ColorMapType);
                        writer.WriteByte((byte)tga.Header.ImageType);
                        writer.WriteShort(tga.Header.ColorMapFirstEntryIndex);
                        writer.WriteShort(tga.Header.ColorMapLength);
                        writer.WriteByte(tga.Header.ColorMapEntrySize);
                        writer.WriteShort(tga.Header.XOrigin);
                        writer.WriteShort(tga.Header.YOrigin);
                        writer.WriteShort(tga.Header.Width);
                        writer.WriteShort(tga.Header.Height);
                        writer.WriteByte(tga.Header.PixelDepth);
                        writer.WriteByte(tga.Header.ImageDescriptor);
                        writer.WriteBytes(tga.Data);
                        tga.ProcessData = new byte[stream.Length];
                        Array.Copy(stream.GetBuffer(), tga.ProcessData, stream.Length);
                    }
                    else if (dds != null && Format == ImageFormat.DDS)
                    {
                        writer.WriteUInt(dds.Header);
                        writer.WriteUInt(dds.Size);
                        writer.WriteUInt(dds.Flags);
                        writer.WriteUInt(dds.Height);
                        writer.WriteUInt(dds.Width);
                        writer.WriteUInt(dds.PitchOrLinearSize);
                        writer.WriteUInt(dds.Depth);
                        writer.WriteUInt(dds.MipMapCount);
                        writer.WriteUInt(dds.AlphaBitDepth);
                        for (int i = 0; i < 10; i++)
                            writer.WriteUInt(dds.Reserved[i]);
                        writer.WriteUInt(dds.StructPixelFormat.Size);
                        writer.WriteUInt(dds.StructPixelFormat.Flags);
                        writer.WriteUInt(dds.StructPixelFormat.FourCC);
                        writer.WriteUInt(dds.StructPixelFormat.RGBBitCount);
                        writer.WriteUInt(dds.StructPixelFormat.RBitMask);
                        writer.WriteUInt(dds.StructPixelFormat.GBitMask);
                        writer.WriteUInt(dds.StructPixelFormat.BBitMask);
                        writer.WriteUInt(dds.StructPixelFormat.ABitMask);
                        writer.WriteUInt(dds.Caps1);
                        writer.WriteUInt(dds.Caps2);
                        writer.WriteUInt(dds.Caps3);
                        writer.WriteUInt(dds.Caps4);
                        writer.WriteUInt(dds.TextureStage);
                        writer.WriteBytes(dds.Data);
                        dds.ProcessData = new byte[stream.Length];
                        Array.Copy(stream.GetBuffer(), dds.ProcessData, stream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
                return false;
            }
            return true;
        }
        public void Dispose()
        {
            if (dds != null)
            {
                Utils.RemoveEvents(dds);
                dds = null;
            }
            if (tga != null)
            {
                Utils.RemoveEvents(tga);
                tga = null;
            }
            Utils.RemoveEvents(handle);
            handle.Close();
            handle.Dispose();
            handle = null;
        }
    }
}