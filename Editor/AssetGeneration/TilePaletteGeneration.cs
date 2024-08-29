using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    internal static class TilePaletteGeneration
    {
        public static void Generate(
            AssetImportContext ctx,
            List<TileSet> tileSets,
            Sprite[] sprites,
            float ppu,
            ref UnityEngine.Object mainAsset)
        {
            if (tileSets.Count == 0 || sprites.Length == 0)
                return;
            
            var textures = GetTextures(ctx);
            var tileSprites = GetTileSprites(tileSets, sprites);
            var templateList = new TileTemplate[textures.Count];
            var tileSet = tileSets[0];
            
            var paletteName = tileSet.name;
            var cellLayout = GridLayout.CellLayout.Rectangle;
            var cellSizing = GridPalette.CellSizing.Manual;
            var cellSize = new Vector3(tileSet.tileSize.x / ppu, tileSet.tileSize.y / ppu, 0);
            var cellSwizzle = GridLayout.CellSwizzle.XYZ;
            var sortMode = TransparencySortMode.Default;
            var sortAxis = Vector3.forward;
            
            var paletteGameObject = GridPaletteUtility.CreateNewPaletteAsSubAsset(
                paletteName,
                cellLayout,
                cellSizing,
                cellSize,
                cellSwizzle,
                sortMode,
                sortAxis,
                textures,
                tileSprites,
                templateList,
                out var palette,
                out var tileAssets
            );
            
            for (var i = 0; i < tileAssets.Count; ++i)
                ctx.AddObjectToAsset(tileAssets[i].name, tileAssets[i]);
            
            ctx.AddObjectToAsset($"{paletteName}_PaletteSettings", palette);
            ctx.AddObjectToAsset(paletteGameObject.name, paletteGameObject);
            mainAsset = paletteGameObject;
        }

        static List<Texture2D> GetTextures(AssetImportContext ctx)
        {
            var assetObjects = new List<Object>();
            ctx.GetObjects(assetObjects);

            var textures = new List<Texture2D>();
            foreach (var obj in assetObjects)
            {
                if (obj is Texture2D texture)
                    textures.Add(texture);
            }
            return textures;            
        }

        static List<Sprite>[] GetTileSprites(List<TileSet> tileSets, Sprite[] sprites)
        {
            var tileSprites = new List<Sprite>(sprites.Length);
            for (var i = 0; i < tileSets.Count; ++i)
            {
                var tiles = tileSets[i].tiles;
                for (var m = 0; m < tiles.Count; ++m)
                {
                    var tile = tiles[m];
                    var spriteIndex = System.Array.FindIndex(sprites, x => x.GetSpriteID() == tile.spriteId);
                    if (spriteIndex == -1)
                        continue;
                    
                    tileSprites.Add(sprites[spriteIndex]);
                }
            }

            return new []{ tileSprites };
        }
    }
}
