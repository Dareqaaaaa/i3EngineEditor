/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 12 jul 2019
 */

using System;

namespace i3EngineEditor.Packet
{
    [Flags]
    public enum ValueEnum : int
    {
        INT32 = 0,
        FLOAT = 1,
        STRING = 2,
        POS1 = 4,
        POS2 = 5,
        HEX = 6
    }
}