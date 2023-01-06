using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    internal static class AnimationClipGeneration
    {
        public static AnimationClip[] Generate(string assetName,
            Sprite[] sprites,
            AsepriteFile file,
            List<Layer> layers,
            List<Tag> tags, 
            Dictionary<int, GameObject> layerIdToGameObject)
        {
            var noOfFrames = file.noOfFrames;
            var animationSpeed = file.animSpeed;
            
            if (tags.Count == 0)
            {
                var tag = new Tag();
                tag.name = assetName + "_Clip";
                tag.fromFrame = 0;
                tag.toFrame = noOfFrames;
                
                tags.Add(tag);
            }
            
            var clips = new AnimationClip[tags.Count];
            for (var i = 0; i < tags.Count; ++i)
                clips[i] = CreateClip(tags[i], layers, sprites, layerIdToGameObject, animationSpeed);

            return clips;
        }

        static AnimationClip CreateClip(Tag tag, List<Layer> layers, Sprite[] sprites, Dictionary<int, GameObject> layerIdToGameObject, int animationSpeed)
        {
            var animationClip = new AnimationClip();
            animationClip.name = tag.name;
            
            var timePerFrameInSec = animationSpeed / 1000f;
            var framesPerSecond = 1f / timePerFrameInSec;
            animationClip.frameRate = framesPerSecond;

            var clipSettings = new AnimationClipSettings();
            clipSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(animationClip, clipSettings);

            for (var i = 0; i < layers.Count; ++i)
            {
                var layer = layers[i];
                var layerGo = layerIdToGameObject[layer.index];

                var spriteKeyframes = new List<ObjectReferenceKeyframe>();
                
                var cells = layer.cells;
                var activeFrames = AddCellsToClip(in cells, in tag, in sprites, timePerFrameInSec, ref spriteKeyframes);

                var linkedCells = layer.linkedCells;
                activeFrames.UnionWith(AddLinkedCellsToClip(in linkedCells, in cells, in tag, in sprites, timePerFrameInSec, ref spriteKeyframes));
                
                var spriteBinding = EditorCurveBinding.PPtrCurve(layerGo.name, typeof(SpriteRenderer), "m_Sprite");
                AnimationUtility.SetObjectReferenceCurve(animationClip, spriteBinding, spriteKeyframes.ToArray());

                AddEnabledKeyframes(layerGo.name, tag, timePerFrameInSec, in activeFrames, in animationClip);
            }

            return animationClip;
        }
        
        static HashSet<int> AddCellsToClip(in List<Cell> cells, in Tag tag, in Sprite[] sprites, float timePerFrameInSec, ref List<ObjectReferenceKeyframe> keyFrames)
        {
            var activeFrames = new HashSet<int>();
            for (var i = 0; i < cells.Count; ++i)
            {
                var cell = cells[i];
                if (cell.frameIndex < tag.fromFrame ||
                    cell.frameIndex >= tag.toFrame)
                    continue;
                
                var sprite = Array.Find(sprites, x => x.GetSpriteID() == cell.spriteId);
                if (sprite == null)
                    continue;
                    
                var keyframe = new ObjectReferenceKeyframe();
                var time = (cell.frameIndex - tag.fromFrame) * timePerFrameInSec;
                keyframe.time = time;
                keyframe.value = sprite;
                keyFrames.Add(keyframe);
                
                activeFrames.Add(cell.frameIndex);
            }
            return activeFrames;
        }

        static HashSet<int> AddLinkedCellsToClip(in List<LinkedCell> linkedCells, in List<Cell> cells, in Tag tag, in Sprite[] sprites, float timePerFrameInSec, ref List<ObjectReferenceKeyframe> keyFrames)
        {
            var activeFrames = new HashSet<int>();
            for (var i = 0; i < linkedCells.Count; ++i)
            {
                var linkedCell = linkedCells[i];
                if (linkedCell.frameIndex < tag.fromFrame ||
                    linkedCell.frameIndex >= tag.toFrame)
                    continue;
                
                var cell = cells.Find(x => x.frameIndex == linkedCell.linkedToFrame);
                if (cell == null)
                    continue;
                    
                var sprite = Array.Find(sprites, x => x.GetSpriteID() == cell.spriteId);
                if (sprite == null)
                    continue;  
                    
                var keyframe = new ObjectReferenceKeyframe();
                var time = (linkedCell.frameIndex - tag.fromFrame) * timePerFrameInSec;
                keyframe.time = time;
                keyframe.value = sprite;
                keyFrames.Add(keyframe);  
                
                activeFrames.Add(linkedCell.frameIndex);
            } 
            return activeFrames;
        }

        static void AddEnabledKeyframes(string layerName, Tag tag, float timePerFrameInSec, in HashSet<int> activeFrames, in AnimationClip animationClip)
        {
            if (activeFrames.Count == tag.noOfFrames)
                return;
            
            var enabledBinding = EditorCurveBinding.FloatCurve(layerName, typeof(SpriteRenderer), "m_Enabled");
            var enabledKeyframes = new List<Keyframe>();

            var disabledPrevFrame = false;
            for (var frameIndex = tag.fromFrame; frameIndex < tag.toFrame; ++frameIndex)
            {
                if (!activeFrames.Contains(frameIndex) && !disabledPrevFrame)
                {
                    var time = (frameIndex - tag.fromFrame) * timePerFrameInSec;
                    var keyframe = GetBoolKeyFrame(false, time);
                    enabledKeyframes.Add(keyframe);
                    disabledPrevFrame = true;
                }
                else if (activeFrames.Contains(frameIndex) && disabledPrevFrame)
                {
                    var time = (frameIndex - tag.fromFrame) * timePerFrameInSec;
                    var keyframe = GetBoolKeyFrame(true, time);
                    enabledKeyframes.Add(keyframe);
                    disabledPrevFrame = false;                        
                }
            }

            if (enabledKeyframes.Count == 0)
                return;

            // Make sure there is an enable keyframe on the first frame, if the first frame is active.
            if (activeFrames.Contains(tag.fromFrame))
            {
                var keyframe = GetBoolKeyFrame(true, 0f);
                enabledKeyframes.Add(keyframe);
            }
            
            var animCurve = new AnimationCurve(enabledKeyframes.ToArray());
            AnimationUtility.SetEditorCurve(animationClip, enabledBinding, animCurve);
        }
        
        static Keyframe GetBoolKeyFrame(bool value, float time)
        {
            var keyframe = new Keyframe();
            keyframe.value = value ? 1f : 0f;
            keyframe.time = time;
            keyframe.inTangent = float.PositiveInfinity;
            keyframe.outTangent = float.PositiveInfinity;
            return keyframe;
        }             
    }
}