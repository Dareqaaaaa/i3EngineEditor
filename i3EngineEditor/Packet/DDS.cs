/* TeamExploit \o/
 *
 * File created by PISTOLA
 * 
 * 26 jul 2019
 */

using System;
using System.Runtime.InteropServices;

namespace i3EngineEditor.Packet
{
    public class DDS
    {
        public uint Header { get; set; }
        public uint Size { get; set; }
        public uint Flags { get; set; }
        public uint Height { get; set; }
        public uint Width { get; set; }
        public uint PitchOrLinearSize { get; set; }
        public uint Depth { get; set; }
        public uint MipMapCount { get; set; }
        public uint AlphaBitDepth { get; set; }
        public uint Surface { get; set; }
        public uint[] Reserved { get; set; } = new uint[40];
        public PixelFormat Format { get; set; }
        public PixelFormatStruct StructPixelFormat;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PixelFormatStruct
        {
            public uint Size { get; set; }
            public uint Flags { get; set; }
            public uint FourCC { get; set; }
            public uint RGBBitCount { get; set; }
            public uint RBitMask { get; set; }
            public uint GBitMask { get; set; }
            public uint BBitMask { get; set; }
            public uint ABitMask { get; set; }
        }
        public uint Caps1 { get; set; }
        public uint Caps2 { get; set; }
        public uint Caps3 { get; set; }
        public uint Caps4 { get; set; }
        public uint TextureStage { get; set; }
        public byte[] Data { get; set; }
        public byte[] ProcessData { get; set; }
        public PixelFormat GetFormat(ref uint blocksize)
        {
            PixelFormat format = PixelFormat.UNKNOWN;
            if ((StructPixelFormat.Flags & DDPF_FOURCC) == DDPF_FOURCC)
            {
                blocksize = ((Width + 3) / 4) * ((Height + 3) / 4) * Depth;
                switch (StructPixelFormat.FourCC)
                {
                    case FOURCC_DXT1:
                        {
                            format = PixelFormat.DXT1;
                            blocksize *= 8;
                            break;
                        }
                    case FOURCC_DXT2:
                        {
                            format = PixelFormat.DXT2;
                            blocksize *= 16;
                            break;
                        }
                    case FOURCC_DXT3:
                        {
                            format = PixelFormat.DXT3;
                            blocksize *= 16;
                            break;
                        }
                    case FOURCC_DXT4:
                        {
                            format = PixelFormat.DXT4;
                            blocksize *= 16;
                            break;
                        }
                    case FOURCC_DXT5:
                        {
                            format = PixelFormat.DXT5;
                            blocksize *= 16;
                            break;
                        }
                    case FOURCC_ATI1:
                        {
                            format = PixelFormat.ATI1N;
                            blocksize *= 8;
                            break;
                        }
                    case FOURCC_ATI2:
                        {
                            format = PixelFormat.THREEDC;
                            blocksize *= 16;
                            break;
                        }
                    case FOURCC_RXGB:
                        {
                            format = PixelFormat.RXGB;
                            blocksize *= 16;
                            break;
                        }
                    case FOURCC_DOLLARNULL:
                        {
                            format = PixelFormat.A16B16G16R16;
                            blocksize = Width * Height * Depth * 8;
                            break;
                        }
                    case FOURCC_oNULL:
                        {
                            format = PixelFormat.R16F;
                            blocksize = Width * Height * Depth * 2;
                            break;
                        }
                    case FOURCC_pNULL:
                        {
                            format = PixelFormat.G16R16F;
                            blocksize = Width * Height * Depth * 4;
                            break;
                        }
                    case FOURCC_qNULL:
                        {
                            format = PixelFormat.A16B16G16R16F;
                            blocksize = Width * Height * Depth * 8;
                            break;
                        }
                    case FOURCC_rNULL:
                        {
                            format = PixelFormat.R32F;
                            blocksize = Width * Height * Depth * 4;
                            break;
                        }
                    case FOURCC_sNULL:
                        {
                            format = PixelFormat.G32R32F;
                            blocksize = Width * Height * Depth * 8;
                            break;
                        }
                    case FOURCC_tNULL:
                        {
                            format = PixelFormat.A32B32G32R32F;
                            blocksize = Width * Height * Depth * 16;
                            break;
                        }
                    default:
                        {
                            format = PixelFormat.UNKNOWN;
                            blocksize *= 16;
                            break;
                        }
                }
            }
            else
            {
                if ((StructPixelFormat.Flags & DDPF_LUMINANCE) == DDPF_LUMINANCE)
                {
                    if ((StructPixelFormat.Flags & DDPF_ALPHAPIXELS) == DDPF_ALPHAPIXELS)
                        format = PixelFormat.LUMINANCE_ALPHA;
                    else
                        format = PixelFormat.LUMINANCE;
                }
                else
                {
                    if ((StructPixelFormat.Flags & DDPF_ALPHAPIXELS) == DDPF_ALPHAPIXELS)
                        format = PixelFormat.RGBA;
                    else
                        format = PixelFormat.RGB;
                }
                blocksize = (Width * Height * Depth * (StructPixelFormat.RGBBitCount >> 3));
            }
            return format;
        }
        #region DDSStruct Flags
        public const uint DDSD_CAPS = 0x00000001;
        public const uint DDSD_Height = 0x00000002;
        public const uint DDSD_Width = 0x00000004;
        public const uint DDSD_PITCH = 0x00000008;
        public const uint DDSD_PIXELFORMAT = 0x00001000;
        public const uint DDSD_MIPMAPCOUNT = 0x00020000;
        public const uint DDSD_LINEARSIZE = 0x00080000;
        public const uint DDSD_Depth = 0x00800000;
        #endregion
        #region pixelformat values
        public const uint DDPF_ALPHAPIXELS = 0x00000001;
        public const uint DDPF_FOURCC = 0x00000004;
        public const uint DDPF_RGB = 0x00000040;
        public const uint DDPF_LUMINANCE = 0x00020000;
        #endregion
        #region ddscaps
        // caps1
        public const uint DDSCAPS_COMPLEX = 0x00000008;
        public const uint DDSCAPS_TEXTURE = 0x00001000;
        public const uint DDSCAPS_MIPMAP = 0x00400000;
        // caps2
        public const uint DDSCAPS2_CUBEMAP = 0x00000200;
        public const uint DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
        public const uint DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
        public const uint DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
        public const uint DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
        public const uint DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
        public const uint DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
        public const uint DDSCAPS2_VOLUME = 0x00200000;
        #endregion
        #region fourccs
        public const uint FOURCC_DXT1 = 0x31545844;
        public const uint FOURCC_DXT2 = 0x32545844;
        public const uint FOURCC_DXT3 = 0x33545844;
        public const uint FOURCC_DXT4 = 0x34545844;
        public const uint FOURCC_DXT5 = 0x35545844;
        public const uint FOURCC_ATI1 = 0x31495441;
        public const uint FOURCC_ATI2 = 0x32495441;
        public const uint FOURCC_RXGB = 0x42475852;
        public const uint FOURCC_DOLLARNULL = 0x24;
        public const uint FOURCC_oNULL = 0x6f;
        public const uint FOURCC_pNULL = 0x70;
        public const uint FOURCC_qNULL = 0x71;
        public const uint FOURCC_rNULL = 0x72;
        public const uint FOURCC_sNULL = 0x73;
        public const uint FOURCC_tNULL = 0x74;
        #endregion
        #region PixelFormat
        [Flags]
        public enum PixelFormat
        {
            RGBA,
            RGB,
            DXT1,
            DXT2,
            DXT3,
            DXT4,
            DXT5,
            THREEDC,
            ATI1N,
            LUMINANCE,
            LUMINANCE_ALPHA,
            RXGB,
            A16B16G16R16,
            R16F,
            G16R16F,
            A16B16G16R16F,
            R32F,
            G32R32F,
            A32B32G32R32F,
            UNKNOWN
        }
        #endregion
    }
}