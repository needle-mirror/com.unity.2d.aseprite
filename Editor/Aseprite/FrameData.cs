using System.IO;
using UnityEngine.Assertions;

namespace UnityEditor.U2D.Aseprite
{
    internal class FrameData
    {
        public uint noOfBytes { get; private set; }
        public ushort frameDuration { get; private set; }
        public uint chunkCount { get; private set; }
        public BaseChunk[] chunks { get; private set; }

        public void Read(BinaryReader reader)
        {
            noOfBytes = reader.ReadUInt32();
            var misc0 = reader.ReadUInt16();
            var legacyChunkCount = reader.ReadUInt16();
            frameDuration = reader.ReadUInt16();
            var misc1 = reader.ReadByte();
            var misc2 = reader.ReadByte();
            var chunkCount = reader.ReadUInt32();  
            
            Assert.IsTrue(misc0 == 0xF1FA, "Reading mismatch.");
            
            this.chunkCount = chunkCount != 0 ? chunkCount : legacyChunkCount;
            chunks = new BaseChunk[this.chunkCount];
        }
    }
}