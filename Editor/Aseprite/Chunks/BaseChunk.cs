using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    internal enum ChunkTypes
    {
        None          = 0,
        OldPalette    = 0x0004,
        OldPalette2   = 0x0011,
        Layer         = 0x2004,
        Cell          = 0x2005,
        CellExtra     = 0x2006,
        ColorProfile  = 0x2007,
        ExternalFiles = 0x2008,
        Mask          = 0x2016,
        Path          = 0x2017,
        Tags          = 0x2018,
        Palette       = 0x2019,
        UserData      = 0x2020,
        Slice         = 0x2022,
        Tileset       = 0x2023
    }
    
    internal class ChunkHeader
    {
        public const int stride = 6;
        
        public uint chunkSize { get; private set; }
        public ChunkTypes chunkType { get; private set; }

        public void Read(BinaryReader reader)
        {
            chunkSize = reader.ReadUInt32();
            chunkType = (ChunkTypes)reader.ReadUInt16();            
        }
    }    
    
    internal abstract class BaseChunk
    {
        public virtual ChunkTypes chunkType => ChunkTypes.None;

        protected uint m_ChunkSize;

        protected BaseChunk(uint chunkSize)
        {
            m_ChunkSize = chunkSize;
        }
        
        public abstract void Read(BinaryReader reader);
    }
}