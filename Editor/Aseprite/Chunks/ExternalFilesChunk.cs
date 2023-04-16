using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class ExternalFilesChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.ExternalFiles;

        public ExternalFilesChunk(uint chunkSize) : base(chunkSize) { }
        protected override void InternalRead(BinaryReader reader) { }
    }
}