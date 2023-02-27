using Unity.Collections;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    internal static class TextureTasks
    {
        public static void AddOpacity(ref NativeArray<Color32> texture, float opacity)
        {
            for (var i = 0; i < texture.Length; ++i)
            {
                var color = texture[i];
                color.a = (byte)(color.a * opacity);
                texture[i] = color;
            }
        }
        
        public static void FlipTextureY(ref NativeArray<Color32> texture, int width, int height)
        {
            if (width == 0 || height == 0)
                return;
            
            var outputTexture = new NativeArray<Color32>(texture.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (var y = 0; y < height; ++y)
            {
                var inRow = ((height - 1) - y) * width;
                var outRow = y * width;
                
                for (var x = 0; x < width; ++x)
                {
                    var inIndex = x + inRow;
                    var outIndex = x + outRow;
                    outputTexture[outIndex] = texture[inIndex];
                }
            }

            texture.DisposeIfCreated();
            texture = outputTexture;
        }

        public static Cell MergeTextures(NativeArray<Color32>[] textures, RectInt[] textureSizes)
        {
            var combinedRect = GetCombinedRect(textureSizes);
            var outputTexture = new NativeArray<Color32>(combinedRect.width * combinedRect.height, Allocator.Persistent);

            var outStartX = combinedRect.x;
            var outStartY = combinedRect.y;
            var outWidth = combinedRect.width;
            var outHeight = combinedRect.height;
            for (var i = 0; i < textures.Length; ++i)
            {
                var inputColor = textures[i];
                var inX = textureSizes[i].x;
                var inY = textureSizes[i].y;
                var inWidth = textureSizes[i].width;
                var inHeight = textureSizes[i].height;

                for (var y = 0; y < inHeight; ++y)
                {
                    var outPosY = (y + inY) - outStartY;
                    // If pixel is outside of output texture's Y, move to the next pixel.
                    if (outPosY < 0 || outPosY >= outHeight)
                        continue;
                    
                    // Flip Y position on the input texture, because
                    // Aseprite textures are stored "upside-down"
                    var inRow = ((inHeight - 1) - y) * inWidth;
                    var outRow = outPosY * outWidth;

                    for (var x = 0; x < inWidth; ++x)
                    {
                        var outPosX = (x + inX) - outStartX;
                        // If pixel is outside of output texture's X, move to the next pixel.
                        if (outPosX < 0 || outPosX >= outWidth)
                            continue;

                        var inBufferIndex = inRow + x;
                        var outBufferIndex = outRow + outPosX;
                        if (outBufferIndex < 0 || outBufferIndex > (outWidth * outHeight))
                            continue;

                        Color inColor = inputColor[inBufferIndex];
                        Color prevOutColor = outputTexture[outBufferIndex];
                        var outColor = new Color();

                        var destAlpha = prevOutColor.a * (1 - inColor.a);
                        outColor.a = inColor.a + prevOutColor.a * (1 - inColor.a);
                        
                        var premultiplyAlpha = outColor.a > 0.0f ? 1 / outColor.a : 1f;
                        outColor.r = (inColor.r * inColor.a + prevOutColor.r * destAlpha) * premultiplyAlpha;
                        outColor.g = (inColor.g * inColor.a + prevOutColor.g * destAlpha) * premultiplyAlpha;
                        outColor.b = (inColor.b * inColor.a + prevOutColor.b * destAlpha) * premultiplyAlpha;

                        outputTexture[outBufferIndex] = outColor;
                    }
                }
            }

            var outputCell = new Cell()
            {
                cellRect = combinedRect,
                image = outputTexture
            };
            return outputCell;
        }

        static RectInt GetCombinedRect(RectInt[] rects)
        {
            var combinedRect = rects[0];
            for (var i = 1; i < rects.Length; ++i)
                FitRectInsideRect(ref combinedRect, in rects[i]);
            return combinedRect;
        }
        
        static void FitRectInsideRect(ref RectInt baseRect, in RectInt rectToFitIn)
        {
            if (baseRect.xMin > rectToFitIn.xMin)
                baseRect.xMin = rectToFitIn.xMin;
            if (baseRect.yMin > rectToFitIn.yMin)
                baseRect.yMin = rectToFitIn.yMin;
            if (baseRect.xMax < rectToFitIn.xMax)
                baseRect.xMax = rectToFitIn.xMax;
            if (baseRect.yMax < rectToFitIn.yMax)
                baseRect.yMax = rectToFitIn.yMax;            
        }
    }
}