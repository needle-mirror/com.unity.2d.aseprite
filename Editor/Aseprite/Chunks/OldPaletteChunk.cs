using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class OldPaletteChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.OldPalette;
        
        public OldPaletteChunk(uint chunkSize) : base(chunkSize) { }
        protected override void InternalRead(BinaryReader reader) { }
    }
}