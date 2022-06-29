/* TeamExploit \o/
 *
 * File created by PISTOLA
 * 
 * 24 jul 2019
 */

using System;
using System.Text;

namespace i3EngineEditor.Packet
{
    [Flags]
    public enum FilesExt : int
    {
        NULL = 0,
        PEF = 1,
        I3GAME = 2,
        I3PACK = 3,
        I3I = 4,
        I3VTEX = 5,
        SIF = 6,
        TEXT = 7,
        ENCRYPT = 8,
        DECRYPT = 9
    }
    public class FilesExtData
    {
        public static string getTypes(int type)
        {
            StringBuilder builder = new StringBuilder();
            if (type == -1)
            {
                builder
                .Append($"PEF Files (*.pef)|*.pef")
                .Append($"|i3Game (*.i3Game)|*.i3Game")
                .Append($"|i3Pack Files (*.i3Pack)|*.i3Pack")
                .Append($"|i3i Files (*.i3i)|*.i3i")
                .Append($"|i3VTex Files (*.i3VTexImage)|*.i3VTexImage")
                .Append($"|lwsi_En Files (*.sif)|*.sif")
                .Append($"|Text Files (*.*)|*.*")
                .Append($"|Encrypt File (*.*)|*.*")
                .Append($"|Decrypt File (*.*)|*.*");
            }
            else
            {
                FilesExt t = (FilesExt)type;
                if (t == FilesExt.PEF)
                    builder.Append($"PEF Decrypted (*.pef)|*.pef|PEF Encrypted (*.pef)|*.pef");
                else if (t == FilesExt.I3GAME)
                    builder.Append($"I3Game Decrypted (*.i3Game)|*.i3Game|I3Game Encrypted (*.i3Game)|*.i3Game");
                else if (t == FilesExt.SIF)
                    builder.Append($"lwsi_En Decrypted (*.sif)|*.sif|lwsi_En Encrypted (*.sif)|*.sif");
                else if (t == FilesExt.I3I)
                    builder.Append($"I3I Decrypted (*.i3i)|*.i3i|I3I Encrypted (*.i3i)|*.i3i");
                else if (t == FilesExt.I3VTEX)
                    builder.Append($"i3VTex Decrypted (*.i3VTexImage)|*.i3VTexImage|i3VTex Encrypted (*.i3VTexImage)|*.i3VTexImage");
                else if (t == FilesExt.ENCRYPT)
                    builder.Append($"File Encrypted (*.*)|*.*");
                else if (t == FilesExt.DECRYPT)
                    builder.Append($"File Decrypted (*.*)|*.*");
            }
            string result = builder.ToString();
            builder = null;
            return result;
        }
        public static string getImageType(ImageFormat format)
        {
            StringBuilder builder = new StringBuilder();
            if (format == ImageFormat.DDS) builder.Append($"DDS Image (*.dds)|*.dds");
            else if (format == ImageFormat.TGA) builder.Append($"TGA Image (*.tga)|*.tga");
            string result = builder.ToString();
            builder = null;
            return result;
        }
        public static FilesExt getExtType(string ext)
        {
            if (ext.Equals(".pef")) return FilesExt.PEF;
            else if (ext.Equals(".i3game")) return FilesExt.I3GAME;
            else if (ext.Equals(".i3pack")) return FilesExt.I3PACK;
            else if (ext.Equals(".i3i")) return FilesExt.I3I;
            else if (ext.Equals(".i3vteximage")) return FilesExt.I3VTEX;
            else if (ext.Equals(".sif")) return FilesExt.SIF;
            else return FilesExt.TEXT;
        }
    }
}