using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    internal static class AnimationClipGeneration
    {
        const string k_RootName = "Root";
        
        public static AnimationClip[] Generate(string assetName,
            Sprite[] sprites,
            AsepriteFile file,
            List<Layer> layers,
            List<Frame> frames,
            List<Tag> tags, 
            Dictionary<int, GameObject> layerIdToGameObject)
        {
            var noOfFrames = file.noOfFrames;
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
                clips[i] = CreateClip(tags[i], layers, frames, sprites, layerIdToGameObject);

            return clips;
        }

        static AnimationClip CreateClip(Tag tag, List<Layer> layers, List<Frame> frames, Sprite[] sprites, Dictionary<int, GameObject> layerIdToGameObject)
        {
            var animationClip = new AnimationClip()
            {
                name = tag.name,
                frameRate = 60f
            };

            var clipSettings = new AnimationClipSettings();
            clipSettings.loopTime = tag.isRepeating;
            AnimationUtility.SetAnimationClipSettings(animationClip, clipSettings);

            for (var i = 0; i < layers.Count; ++i)
            {
                var layer = layers[i];
                if (layer.layerType != LayerTypes.Normal)
                    continue;
                
                var layerGo = layerIdToGameObject[layer.index];

                var spriteKeyframes = new List<ObjectReferenceKeyframe>();
                
                var cells = layer.cells;
                var activeFrames = AddCellsToClip(in cells, in tag, in sprites, in frames, ref spriteKeyframes);

                var linkedCells = layer.linkedCells;
                activeFrames.UnionWith(AddLinkedCellsToClip(in linkedCells, in cells, in tag, in sprites, in frames, ref spriteKeyframes));

                spriteKeyframes.Sort((x, y) => x.time.CompareTo(y.time));
                DuplicateLastFrame(ref spriteKeyframes, frames[^1]);

                var path = GetGameObjectPath(layerGo.transform);
                var spriteBinding = EditorCurveBinding.PPtrCurve(path, typeof(SpriteRenderer), "m_Sprite");
                AnimationUtility.SetObjectReferenceCurve(animationClip, spriteBinding, spriteKeyframes.ToArray());

                AddEnabledKeyframes(layerGo, tag, in frames, in activeFrames, in animationClip);
            }

            return animationClip;
        }

        static HashSet<int> AddCellsToClip(in List<Cell> cells, in Tag tag, in Sprite[] sprites, in List<Frame> frames, ref List<ObjectReferenceKeyframe> keyFrames)
        {
            var activeFrames = new HashSet<int>();
            var startTime = GetTimeFromFrame(in frames, tag.fromFrame);
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
                var time = GetTimeFromFrame(in frames, cell.frameIndex);
                keyframe.time = time - startTime;
                keyframe.value = sprite;
                keyFrames.Add(keyframe);
                
                activeFrames.Add(cell.frameIndex);
            }
            return activeFrames;
        }

        static HashSet<int> AddLinkedCellsToClip(in List<LinkedCell> linkedCells, in List<Cell> cells, in Tag tag, in Sprite[] sprites, in List<Frame> frames, ref List<ObjectReferenceKeyframe> keyFrames)
        {
            var activeFrames = new HashSet<int>();
            var startTime = GetTimeFromFrame(in frames, tag.fromFrame);
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
                var time = GetTimeFromFrame(in frames, linkedCell.frameIndex);
                keyframe.time = time - startTime;
                keyframe.value = sprite;
                keyFrames.Add(keyframe);  
                
                activeFrames.Add(linkedCell.frameIndex);
            } 
            return activeFrames;
        }

        static void DuplicateLastFrame(ref List<ObjectReferenceKeyframe> keyFrames, Frame lastFrame)
        {
            if (keyFrames.Count == 0)
                return;
            
            var lastKeyFrame = keyFrames[^1];
            var duplicatedFrame = new ObjectReferenceKeyframe();
            duplicatedFrame.time = lastKeyFrame.time + MsToSeconds(lastFrame.duration);
            duplicatedFrame.value = lastKeyFrame.value;
            keyFrames.Add(duplicatedFrame); 
        }

        static string GetGameObjectPath(Transform transform)
        {
            var path = transform.name;
            if (transform.name == k_RootName)
                return "";
            if (transform.parent.name == k_RootName)
                return path;
            
            var parentPath = GetGameObjectPath(transform.parent) + "/";
            path = path.Insert(0, parentPath);
            return path;
        }

        static void AddEnabledKeyframes(GameObject layerGo, Tag tag, in List<Frame> frames, in HashSet<int> activeFrames, in AnimationClip animationClip)
        {
            if (activeFrames.Count == tag.noOfFrames)
                return;
            
            var path = GetGameObjectPath(layerGo.transform);
            var enabledBinding = EditorCurveBinding.FloatCurve(path, typeof(SpriteRenderer), "m_Enabled");
            var enabledKeyframes = new List<Keyframe>();

            var disabledPrevFrame = false;
            var startTime = GetTimeFromFrame(in frames, tag.fromFrame);
            for (var frameIndex = tag.fromFrame; frameIndex < tag.toFrame; ++frameIndex)
            {
                var time = GetTimeFromFrame(in frames, frameIndex);
                time -= startTime;

                if (!activeFrames.Contains(frameIndex) && !disabledPrevFrame)
                {
                    var keyframe = GetBoolKeyFrame(false, time);
                    enabledKeyframes.Add(keyframe);
                    disabledPrevFrame = true;
                }
                else if (activeFrames.Contains(frameIndex) && disabledPrevFrame)
                {
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

        static float GetTimeFromFrame(in List<Frame> frames, int frameIndex)
        {
            var totalMs = 0;
            for (var i = 0; i < frameIndex; ++i)
                totalMs += frames[i].duration;
            return MsToSeconds(totalMs);
        }

        static float MsToSeconds(int ms) => ms / 1000f;

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