using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class SliceChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Slice;

        public SliceChunk(uint chunkSize) : base(chunkSize) { }
        protected override void InternalRead(BinaryReader reader) { }
    }
}