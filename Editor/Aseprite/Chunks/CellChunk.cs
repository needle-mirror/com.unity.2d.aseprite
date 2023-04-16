using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEditor.U2D.Aseprite
{
    internal enum CellTypes
    {
        RawImage = 0,
        LinkedCel = 1,
        CompressedImage = 2,
        CompressedTileMap = 3
    }
    
    internal class CellChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Cell;
        
        public CellChunk(uint chunkSize, ushort colorDepth, PaletteChunk paletteChunk, byte alphaPaletteEntry) : base(chunkSize)
        {
            m_ColorDepth = colorDepth;
            m_PaletteChunk = paletteChunk;
            m_AlphaPaletteEntry = alphaPaletteEntry;
        }

        readonly ushort m_ColorDepth;
        readonly PaletteChunk m_PaletteChunk;
        readonly byte m_AlphaPaletteEntry;
        
        public ushort layerIndex { get; private set; }
        public short posX { get; private set; }
        public short posY { get; private set; }
        public byte opacity { get; private set; }
        public CellTypes cellType { get; private set; }
        public int linkedToFrame { get; private set; } = -1;
        public ushort width { get; private set; }
        public ushort height { get; private set; }
        public NativeArray<Color32> image { get; private set; }
        public UserDataChunk dataChunk { get; set; }

        protected override void InternalRead(BinaryReader reader)
        {
            layerIndex = reader.ReadUInt16();
            posX = reader.ReadInt16();
            posY = reader.ReadInt16();
            opacity = reader.ReadByte();
            cellType = (CellTypes)reader.ReadUInt16();
            
            // Not in use bytes
            for (var i = 0; i < 7; ++i)
            {
                var miscVal = reader.ReadByte();
                Assert.IsTrue(miscVal == 0);
            }
            
            // 16 bytes read so far.

            if (cellType == CellTypes.RawImage)
            {
                width = reader.ReadUInt16();
                height = reader.ReadUInt16();

                byte[] imageData = null;
                if (m_ColorDepth == 32)
                    imageData = reader.ReadBytes(width * height * 4);
                else if (m_ColorDepth == 16)
                    imageData = reader.ReadBytes(width * height * 2);
                else if (m_ColorDepth == 8)
                    imageData = reader.ReadBytes(width * height);

                if (imageData != null)
                    image = AsepriteUtilities.GenerateImageData(m_ColorDepth, imageData, m_PaletteChunk, m_AlphaPaletteEntry);

            }
            else if (cellType == CellTypes.LinkedCel)
            {
                linkedToFrame = reader.ReadUInt16();
            }
            else if (cellType == CellTypes.CompressedImage)
            {
                width = reader.ReadUInt16();
                height = reader.ReadUInt16();

                var dataSize = (int)m_ChunkSize - ChunkHeader.stride - 20;
                var decompressedData = AsepriteUtilities.ReadAndDecompressedData(reader, dataSize);
                
                image = AsepriteUtilities.GenerateImageData(m_ColorDepth, decompressedData, m_PaletteChunk, m_AlphaPaletteEntry);
            }
            else if (cellType == CellTypes.CompressedTileMap) // Not implemented yet.
            {
                width = reader.ReadUInt16();
                height = reader.ReadUInt16();
                var bitsPerTile = reader.ReadUInt16();
                var tileIdMask = reader.ReadUInt32();
                var xFlipMask = reader.ReadUInt32();
                var yFlipMask = reader.ReadUInt32();
                var rotation90Mask = reader.ReadUInt32();
                
                // Not in use bytes
                for (var i = 0; i < 10; ++i)
                    reader.ReadByte();
                
                var dataSize = (int)m_ChunkSize - ChunkHeader.stride - 48;
                var decompressedData = AsepriteUtilities.ReadAndDecompressedData(reader, dataSize);

                var bytesPerTile = bitsPerTile / 8;
                var noOfTiles = decompressedData.Length / bytesPerTile;

                using var memoryStream = new MemoryStream(decompressedData);
                using var binaryReader = new BinaryReader(memoryStream);
                for (var i = 0; i < noOfTiles; ++i)
                {
                    uint tileIndex = 0;
                    if (bitsPerTile == 32)
                        tileIndex = binaryReader.ReadUInt32();
                    else if (bitsPerTile == 16)
                        tileIndex = binaryReader.ReadUInt16();
                    else if (bitsPerTile == 8)
                        tileIndex = binaryReader.ReadByte();
                }
            }
        }
    }    
}