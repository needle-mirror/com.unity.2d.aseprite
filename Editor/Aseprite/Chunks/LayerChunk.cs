using System;
using System.IO;

namespace UnityEditor.U2D.Aseprite
{
    [Flags]
    internal enum LayerFlags
    {
        Visible = 1, 
        Editable = 2, 
        LockMovement = 4, 
        Background = 8, 
        PreferLinkedCels = 16,
        DisplayAsCollapsed = 32,
        ReferenceLayer = 64        
    }   
    
    internal enum LayerTypes
    {
        Normal = 0,
        Group = 1,
        Tilemap = 2
    }
    
    internal enum BlendModes
    {
        Normal         = 0,
        Multiply       = 1,
        Screen         = 2,
        Overlay        = 3,
        Darken         = 4,
        Lighten        = 5,
        ColorDodge     = 6,
        ColorBurn      = 7,
        HardLight      = 8,
        SoftLight      = 9,
        Difference     = 10,
        Exclusion      = 11,
        Hue            = 12,
        Saturation     = 13,
        Color          = 14,
        Luminosity     = 15,
        Addition       = 16,
        Subtract       = 17,
        Divide         = 18        
    }    
    
    internal class LayerChunk : BaseChunk
    {
        public override ChunkTypes chunkType => ChunkTypes.Layer;

        public LayerFlags flags { get; private set; }
        public LayerTypes layerType { get; private set; }
        public ushort childLevel { get; private set; }
        public BlendModes blendMode { get; private set; }
        public byte opacity { get; private set; }
        public string name { get; private set; }
        public uint tileSetIndex { get; private set; }
        
        public LayerChunk(uint chunkSize) : base(chunkSize) { }

        public override void Read(BinaryReader reader)
        {
            flags = (LayerFlags)reader.ReadUInt16();
            layerType = (LayerTypes)reader.ReadUInt16();
            childLevel = reader.ReadUInt16();
            var defaultLayerWidth = reader.ReadUInt16();
            var defaultLayerHeight = reader.ReadUInt16();
            blendMode = (BlendModes)reader.ReadUInt16();
            opacity = reader.ReadByte();
            
            // Not in use bytes
            for (var i = 0; i < 3; ++i)
                reader.ReadByte();
            
            name = AsepriteUtilities.ReadString(reader);
            if (layerType == LayerTypes.Tilemap)
                tileSetIndex = reader.ReadUInt32();
        }
    }
}