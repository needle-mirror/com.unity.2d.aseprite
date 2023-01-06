using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    internal static class AsepriteUtilities
    {
        public static string ReadString(BinaryReader reader)
        {
            var strLength = reader.ReadUInt16();
            var text = "";
            for (var m = 0; m < strLength; ++m)
            {
                var character = (char)reader.ReadByte();
                text += character;
            }
            return text;
        }
    }
}