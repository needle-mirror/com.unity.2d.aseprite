using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class NoneChunk : BaseChunk
    {
        public NoneChunk(uint chunkSize) : base(chunkSize) { }
        protected override void InternalRead(BinaryReader reader) { }
    }
}