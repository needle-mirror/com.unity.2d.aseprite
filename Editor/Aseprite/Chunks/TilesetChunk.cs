using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class TilesetChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Tileset;

        public TilesetChunk(uint chunkSize) : base(chunkSize) { }

        public override void Read(BinaryReader reader)
        {
            var bytes = reader.ReadBytes((int)m_ChunkSize - ChunkHeader.stride);
        }
    }
}