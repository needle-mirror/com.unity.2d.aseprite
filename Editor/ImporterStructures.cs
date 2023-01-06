using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    [Serializable]
    internal class Layer
    {
        [SerializeField] int m_LayerIndex;
        [SerializeField] string m_Name;
        [SerializeField] LayerFlags m_LayerFlags;
        [SerializeField] LayerTypes m_LayerType;
        [SerializeField] List<Cell> m_Cells = new List<Cell>();
        [SerializeField] List<LinkedCell> m_LinkedCells = new List<LinkedCell>();
        [SerializeField] Layer m_ParentLayer;

        public int index
        {
            get => m_LayerIndex;
            set => m_LayerIndex = value;
        }
        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }
        public LayerFlags layerFlags
        {
            get => m_LayerFlags;
            set => m_LayerFlags = value;
        }
        public LayerTypes layerType
        {
            get => m_LayerType;
            set => m_LayerType = value;
        }
        public List<Cell> cells
        {
            get => m_Cells;
            set => m_Cells = value;
        }
        public List<LinkedCell> linkedCells
        {
            get => m_LinkedCells;
            set => m_LinkedCells = value;
        }
        public Layer parentLayer
        {
            get => m_ParentLayer;
            set => m_ParentLayer = value;
        }
    }

    [Serializable]
    internal class Cell
    {
        [SerializeField] string m_Name;
        [SerializeField] int m_FrameIndex;
        [SerializeField] RectInt m_CellRect;
        [SerializeField] string m_SpriteId;

        [NonSerialized] public bool updatedCellRect = false;
        [NonSerialized] public NativeArray<Color32> image;

        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }
        public int frameIndex
        {
            get => m_FrameIndex;
            set => m_FrameIndex = value;
        }
        public RectInt cellRect
        {
            get => m_CellRect;
            set => m_CellRect = value;
        }
        public GUID spriteId
        {
            get => new GUID(m_SpriteId);
            set => m_SpriteId = value.ToString();
        }
    }

    [Serializable]
    internal class LinkedCell
    {
        [SerializeField] int m_FrameIndex;
        [SerializeField] int m_LinkedToFrame;
        
        public int frameIndex
        {
            get => m_FrameIndex;
            set => m_FrameIndex = value;
        }
        public int linkedToFrame
        {
            get => m_LinkedToFrame;
            set => m_LinkedToFrame = value;
        }
    }

    internal class Tag
    {
        string m_Name;
        int m_FromFrame;
        int m_ToFrame;

        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }
        
        public int fromFrame
        {
            get => m_FromFrame;
            set => m_FromFrame = value;
        }
        public int toFrame
        {
            get => m_ToFrame;
            set => m_ToFrame = value;
        }

        public int noOfFrames => toFrame - fromFrame;
    }

    /// <summary>
    /// Import modes for all layers.
    /// </summary>
    public enum LayerImportModes
    {
        /// <summary>
        /// Every layer per frame generates a Sprite.
        /// </summary>
        Individual,
        /// <summary>
        /// All layers per frame are merged into one Sprite.
        /// </summary>
        Merged
    }

    /// <summary>
    /// The space the Sprite pivots are being calculated.
    /// </summary>
    public enum PivotSpaces
    {
        /// <summary>
        /// Canvas space. Calculate the pivot based on where the Sprite is positioned on the source asset's canvas.
        /// This is useful if the Sprite is being swapped out in an animation.
        /// </summary>
        Canvas,
        /// <summary>
        /// Local space. This is the normal pivot space. 
        /// </summary>
        Local
    }
}