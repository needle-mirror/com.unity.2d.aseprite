using System.IO;
using UnityEngine.Assertions;

namespace UnityEditor.U2D.Aseprite
{
    internal class AsepriteFile
    {
        public uint fileSize { get; private set; }
        public ushort noOfFrames { get; private set; }
        public ushort width { get; private set; }
        public ushort height { get; private set; }
        public ushort colorDepth { get; private set; }
        public uint flags { get; private set; }
        /// <summary>
        /// Time per frame (in milliseconds)
        /// </summary>
        public ushort animSpeed { get; private set; }
        public byte alphaPaletteEntry { get; private set; }
        public ushort noOfColors { get; private set; }
        public byte pixelWidth { get; private set; }
        public byte pixelHeight { get; private set; }
        public short gridPosX { get; private set; }
        public short gridPosY { get; private set; }
        public ushort gridWidth { get; private set; }
        public ushort gridHeight { get; private set; }
        public FrameData[] frameData { get; private set; }

        public void Read(BinaryReader reader)
        {
            fileSize = reader.ReadUInt32();
            var misc0 = reader.ReadUInt16();
            noOfFrames = reader.ReadUInt16();
            width = reader.ReadUInt16();
            height = reader.ReadUInt16();
            colorDepth = reader.ReadUInt16();
            flags = reader.ReadUInt32();
            animSpeed = reader.ReadUInt16();
            var misc1 = reader.ReadUInt32();
            var misc2 = reader.ReadUInt32();
            alphaPaletteEntry = reader.ReadByte();
            var miscByte0 = reader.ReadByte();
            var miscByte1 = reader.ReadByte();
            var miscByte2 = reader.ReadByte();
            noOfColors = reader.ReadUInt16();
            pixelWidth = reader.ReadByte();
            pixelHeight = reader.ReadByte();
            gridPosX = reader.ReadInt16();
            gridPosY = reader.ReadInt16();
            gridWidth = reader.ReadUInt16();
            gridHeight = reader.ReadUInt16();
            
            Assert.IsTrue(misc0 == 0xA5E0, "Unexpected file content. The file is most likely corrupt.");
            
            // Unused 84 bytes
            for (var i = 0; i < 84; ++i)
                reader.ReadByte();

            frameData = new FrameData[noOfFrames];
        }
    }
}