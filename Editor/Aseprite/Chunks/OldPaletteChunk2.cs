using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class OldPaletteChunk2 : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.OldPalette2;

        public OldPaletteChunk2(uint chunkSize) : base(chunkSize) { }
        protected override void InternalRead(BinaryReader reader) { }
    }
}