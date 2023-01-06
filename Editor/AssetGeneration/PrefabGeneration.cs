using System;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    internal static class PrefabGeneration
    {
        public static void Generate(
            AssetImportContext ctx, 
            TextureGenerationOutput output, 
            List<Layer> layers,
            Dictionary<int, GameObject> layerIdToGameObject,
            Vector2Int canvasSize,
            AsepriteImporterSettings importSettings,
            ref UnityEngine.Object mainAsset, 
            out GameObject rootGameObject)
        {
            var globalPivot = ImportUtilities.PivotAlignmentToVector(importSettings.defaultPivotAlignment);
            
            rootGameObject = new GameObject("Root");
#if ENABLE_URP
            if (importSettings.addShadowCasters && layers.Count > 1)
                rootGameObject.AddComponent<UnityEngine.Rendering.Universal.CompositeShadowCaster2D>(); 
#endif            
            
            for (var i = 0; i < layers.Count; ++i)
            {
                var layer = layers[i];
                var go = new GameObject(layer.name);
                go.transform.parent = rootGameObject.transform;
                go.transform.localRotation = Quaternion.identity;
                
                layerIdToGameObject.Add(layer.index, go);

                var firstCell = layer.cells.Count > 0 ? layer.cells[0] : null;
                if (firstCell != null)
                {
                    var sprite = Array.Find(output.sprites, x => x.GetSpriteID() == firstCell.spriteId);
                    
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = layer.index;
                    
#if ENABLE_URP
                    if (importSettings.addShadowCasters)
                        go.AddComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>(); 
#endif

                    if (importSettings.defaultPivotSpace == PivotSpaces.Canvas)
                        go.transform.localPosition = Vector3.zero;
                    else
                    {
                        var cellRect = firstCell.cellRect;
                        
                        var position = new Vector3(cellRect.x, cellRect.y, 0f);
                        
                        var pivot = sprite.pivot;
                        position.x += pivot.x;
                        position.y += pivot.y;

                        position.x -= (canvasSize.x * globalPivot.x);
                        position.y -= (canvasSize.y * globalPivot.y);
                        
                        position.x /= sprite.pixelsPerUnit;
                        position.y /= sprite.pixelsPerUnit;

                        go.transform.localPosition = position;
                    }
                }
            }

            // We need the GameObjects in order to generate Animation Clips.
            // But we will only save down the GameObjects if it is requested.
            if (importSettings.generateModelPrefab)
            {
                ctx.AddObjectToAsset(rootGameObject.name, rootGameObject);
                mainAsset = rootGameObject;
            }
            else
                rootGameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}