using System;
using System.IO;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    [Flags]
    internal enum UserDataFlags
    {
        HasText = 1,
        HasColor = 2
    }
    
    internal class UserDataChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.UserData;
        
        public string text { get; private set; }
        public Color32 color { get; private set; }

        public UserDataChunk(uint chunkSize) : base(chunkSize) { }
        
        public override void Read(BinaryReader reader)
        {
            var flag = (UserDataFlags) reader.ReadUInt32();

            if ((flag & UserDataFlags.HasText) != 0)
                text = AsepriteUtilities.ReadString(reader);
            if ((flag & UserDataFlags.HasColor) != 0)
            {
                var red = reader.ReadByte();
                var green = reader.ReadByte();
                var blue = reader.ReadByte();
                var alpha = reader.ReadByte();
                color = new Color32(red, green, blue, alpha);
            }
        }
    }
}