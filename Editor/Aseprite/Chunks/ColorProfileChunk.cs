using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    internal enum ColorProfileTypes
    {
        NoProfile   = 0,
        sRGB        = 1,
        ICC         = 2
    }    
    
    internal class ColorProfileChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.ColorProfile;

        public ColorProfileTypes profileType { get; private set; }
        public ushort flags { get; private set; }
        public float gamma { get; private set; }
        
        public ColorProfileChunk(uint chunkSize) : base(chunkSize) { }

        public override void Read(BinaryReader reader)
        {
            profileType = (ColorProfileTypes)reader.ReadUInt16();
            flags = reader.ReadUInt16();
            gamma = reader.ReadSingle();

            // Reserved bytes
            for (var i = 0; i < 8; ++i)
                reader.ReadByte();

            if (profileType == ColorProfileTypes.ICC)
            {
                var iccProfileLength = reader.ReadUInt32();
                for (var i = 0; i < iccProfileLength; ++i)
                    reader.ReadByte();
            }
        }
    }
}