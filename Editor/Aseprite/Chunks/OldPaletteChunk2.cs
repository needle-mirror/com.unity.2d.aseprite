using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    /// <summary>Not supported yet.</summary>
    internal class OldPaletteChunk2 : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.OldPalette2;

        public OldPaletteChunk2(uint chunkSize) : base(chunkSize) { }

        public override void Read(BinaryReader reader)
        {
            var bytes = reader.ReadBytes((int)m_ChunkSize - ChunkHeader.stride);
        }
    }
}