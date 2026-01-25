using UnityEngine;
using UnityEditor.Animations;

namespace UnityEditor.U2D.Aseprite.Common
{
    internal static class AnimatorControllerHelper
    {
#if UNITY_6000_5_OR_NEWER            
        [Callbacks.OnOpenAsset]
        static bool OnOpenAsset(EntityId entityId, int line)
        {
            var controller = EditorUtility.EntityIdToObject(entityId) as AnimatorController;
            if (controller)
            {
                EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
                return true;
            }
            return false;
        } 
#else        
        [Callbacks.OnOpenAsset]
        static bool OnOpenAsset(int instanceID, int line)
        {
            var controller = EditorUtility.InstanceIDToObject(instanceID) as AnimatorController;
            if (controller)
            {
                EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
                return true;
            }
            return false;
        }
#endif
    }
}
