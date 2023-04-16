using System;
using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    [Flags]
    internal enum TileSetFlags
    {
        IncludesLinkToExternal = 1, 
        IncludesTilesInFile = 2, 
        Misc = 4,
    }     
    
    internal class TilesetChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Tileset;

        public uint tileSetId  { get; private set; }
        public TileSetFlags tileSetFlags { get; private set; }
        public uint noOfTiles { get; private set; }
        public ushort width { get; private set; }
        public ushort height { get; private set; }
        public string tileSetName { get; private set; }

        readonly ushort m_ColorDepth;
        readonly PaletteChunk m_PaletteChunk;
        readonly byte m_AlphaPaletteEntry;

        public TilesetChunk(uint chunkSize, ushort colorDepth, PaletteChunk paletteChunk, byte alphaPaletteEntry) : base(chunkSize)
        {
            m_ColorDepth = colorDepth;
            m_PaletteChunk = paletteChunk;
            m_AlphaPaletteEntry = alphaPaletteEntry;
        }

        protected override void InternalRead(BinaryReader reader)
        {
            tileSetId = reader.ReadUInt32();
            tileSetFlags = (TileSetFlags)reader.ReadUInt32();
            noOfTiles = reader.ReadUInt32();
            width = reader.ReadUInt16();
            height = reader.ReadUInt16();
            
            var baseIndex = reader.ReadInt16();
            var reservedBytes = reader.ReadBytes(14);

            tileSetName = AsepriteUtilities.ReadString(reader);
            
            // Not supported yet.
            if ((tileSetFlags & TileSetFlags.IncludesLinkToExternal) != 0)
            {
                var idOfExternalFile = reader.ReadUInt32();
                var tileSetIdInExternal = reader.ReadUInt32();
            }
            if ((tileSetFlags & TileSetFlags.IncludesTilesInFile) != 0)
            {
                var compressedDataLength = (int)reader.ReadUInt32();
                var decompressedData = AsepriteUtilities.ReadAndDecompressedData(reader, compressedDataLength);
                
                var image = AsepriteUtilities.GenerateImageData(m_ColorDepth, decompressedData, m_PaletteChunk, m_AlphaPaletteEntry);                
            }
        }
    }
}