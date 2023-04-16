using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class PathChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Path;

        public PathChunk(uint chunkSize) : base(chunkSize) { }
        protected override void InternalRead(BinaryReader reader) { }
    }
}