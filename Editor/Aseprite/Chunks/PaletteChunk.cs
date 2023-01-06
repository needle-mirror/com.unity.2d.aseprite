using System.IO;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    internal struct PaletteEntry
    {
        public string name;
        public Color32 color;
    }    
    
    internal class PaletteChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Palette;
        
        public uint noOfEntries { get; private set; }
        public uint firstColorIndex { get; private set; }
        public uint lastColorIndex { get; private set; }
        public PaletteEntry[] entries { get; private set; }
        
        public PaletteChunk(uint chunkSize) : base(chunkSize) { }
        
        public override void Read(BinaryReader reader)
        {
            noOfEntries = reader.ReadUInt32();
            firstColorIndex = reader.ReadUInt32();
            lastColorIndex = reader.ReadUInt32();
            
            // Reserved bytes
            for (var i = 0; i < 8; ++i)
                reader.ReadByte();

            entries = new PaletteEntry[noOfEntries];
            for (var i = 0; i < noOfEntries; ++i)
            {
                var entryFlag = reader.ReadUInt16();
                var red = reader.ReadByte();
                var green = reader.ReadByte();
                var blue = reader.ReadByte();
                var alpha = reader.ReadByte();

                entries[i].color = new Color32(red, green, blue, alpha);
                entries[i].name = "";

                var hasName = entryFlag == 1;
                if (hasName)
                    entries[i].name = AsepriteUtilities.ReadString(reader);
            }
        }
    }
}