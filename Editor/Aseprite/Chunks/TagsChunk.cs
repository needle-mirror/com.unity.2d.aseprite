using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    internal enum LoopAnimationDirection
    {
        Forward = 0,
        Reverse = 1,
        PingPong = 2,
        PingPongReverse = 3,
    }    
    
    internal class TagData
    {
        public ushort fromFrame { get; set; }
        public ushort toFrame { get; set; }
        public LoopAnimationDirection loopDirection { get; set; }
        public ushort noOfRepeats { get; set; }
        public string name { get; set; }
    }    
    
    internal class TagsChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Tags;

        public int noOfTags { get; private set; }
        public TagData[] tagData { get; private set; }

        public TagsChunk(uint chunkSize) : base(chunkSize) { }

        protected override void InternalRead(BinaryReader reader)
        {
            noOfTags = reader.ReadUInt16();

            // Not in use bytes
            for (var i = 0; i < 8; ++i)
                reader.ReadByte();

            tagData = new TagData[noOfTags];
            for (var i = 0; i < noOfTags; ++i)
            {
                tagData[i] = new TagData();
                tagData[i].fromFrame = reader.ReadUInt16();
                tagData[i].toFrame = reader.ReadUInt16();
                tagData[i].loopDirection = (LoopAnimationDirection) reader.ReadByte();
                tagData[i].noOfRepeats = reader.ReadUInt16();
                
                // Not in use bytes
                for (var m = 0; m < 6; ++m)
                    reader.ReadByte();
                // Tag color. Deprecated.
                for (var m = 0; m < 3; ++m)
                    reader.ReadByte();
                reader.ReadByte();
                
                tagData[i].name = AsepriteUtilities.ReadString(reader);
            }
        }
    }
}