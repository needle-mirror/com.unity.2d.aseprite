using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class CellExtra : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.CellExtra;
        public CellExtra(uint chunkSize) : base(chunkSize) { }
        protected override void InternalRead(BinaryReader reader) { }
    }
}