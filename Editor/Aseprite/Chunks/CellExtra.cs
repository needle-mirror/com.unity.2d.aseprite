using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class CellExtra : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.CellExtra;

        public CellExtra(uint chunkSize) : base(chunkSize) { }

        public override void Read(BinaryReader reader)
        {
            var bytes = reader.ReadBytes((int)m_ChunkSize - ChunkHeader.stride);
        }
    }
}