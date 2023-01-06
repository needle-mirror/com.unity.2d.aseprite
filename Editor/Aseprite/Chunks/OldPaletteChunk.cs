using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class OldPaletteChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.OldPalette;

        public ushort noOfEntries { get; private set; }
        public List<Color32> colors { get; } = new List<Color32>();
        
        public OldPaletteChunk(uint chunkSize) : base(chunkSize) { }

        public override void Read(BinaryReader reader)
        {
            var bytes = reader.ReadBytes((int)m_ChunkSize - ChunkHeader.stride);
            
            // noOfEntries = reader.ReadUInt16();
            // for (var i = 0; i < noOfEntries; ++i)
            // {
            //     var noToSkip = reader.ReadByte();
            //     
            //     var noOfColors = (int)reader.ReadByte();
            //     if (noOfColors == 0)
            //         noOfColors = 256;
            //     
            //     for (var m = 0; m < noOfColors; ++m)
            //     {
            //         var red = reader.ReadByte();
            //         var green = reader.ReadByte();
            //         var blue = reader.ReadByte();
            //         colors.Add(new Color32(red, green, blue, 255));
            //     }
            // }
        }
    }
}