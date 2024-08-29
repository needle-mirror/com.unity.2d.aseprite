using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    internal static class ImportMergedLayers
    {
        public static void Import(string assetName, List<Layer> layers, out List<NativeArray<Color32>> cellBuffers, out List<int2> cellSize)
        {
            var cellsPerFrame = CellTasks.GetAllCellsPerFrame(in layers);
            var mergedCells = CellTasks.MergeCells(in cellsPerFrame, assetName);

            CellTasks.CollectDataFromCells(mergedCells, out cellBuffers, out cellSize);
            UpdateLayerList(mergedCells, assetName, layers);
        }

        static void UpdateLayerList(List<Cell> cells, string assetName, List<Layer> layers)
        {
            layers.Clear();
            var flattenLayer = new Layer()
            {
                cells = cells,
                index = 0,
                name = assetName
            };
            flattenLayer.guid = Layer.GenerateGuid(flattenLayer, layers);
            layers.Add(flattenLayer);
        }
    }
}
