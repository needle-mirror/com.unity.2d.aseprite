using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class PathChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Path;

        public PathChunk(uint chunkSize) : base(chunkSize) { }

        public override void Read(BinaryReader reader)
        {
            var bytes = reader.ReadBytes((int)m_ChunkSize - ChunkHeader.stride);
        }
    }
}