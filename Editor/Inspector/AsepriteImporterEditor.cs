#pragma warning disable 0219

using System;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEditor.U2D.Aseprite.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Aseprite
{
    [CustomEditor(typeof(AsepriteImporter))]
    [CanEditMultipleObjects]
    internal class AsepriteImporterEditor : ScriptedImporterEditor, ITexturePlatformSettingsDataProvider
    {
        struct InspectorGUI
        {
            public VisualElement container;
            public bool needsRepaint;
            public Action onUpdate;
            public Action onUIActivated;
        }

        SerializedProperty m_TextureType;
        SerializedProperty m_TextureShape;
        SerializedProperty m_SpriteMode;
        SerializedProperty m_SpritePixelsToUnits;
        SerializedProperty m_SpriteMeshType;
        SerializedProperty m_SpriteExtrude;
        SerializedProperty m_Alignment;
        SerializedProperty m_SpritePivot;
        SerializedProperty m_NPOTScale;
        SerializedProperty m_IsReadable;
        SerializedProperty m_sRGBTexture;
        SerializedProperty m_AlphaSource;
#if ENABLE_TEXTURE_STREAMING
        SerializedProperty m_StreamingMipmaps;
        SerializedProperty m_StreamingMipmapsPriority;
#endif
        SerializedProperty m_MipMapMode;
        SerializedProperty m_EnableMipMap;
        SerializedProperty m_FadeOut;
        SerializedProperty m_BorderMipMap;
        SerializedProperty m_MipMapsPreserveCoverage;
        SerializedProperty m_AlphaTestReferenceValue;
        SerializedProperty m_MipMapFadeDistanceStart;
        SerializedProperty m_MipMapFadeDistanceEnd;
        SerializedProperty m_AlphaIsTransparency;
        SerializedProperty m_FilterMode;
        SerializedProperty m_Aniso;

        SerializedProperty m_WrapU;
        SerializedProperty m_WrapV;
        SerializedProperty m_WrapW;
        SerializedProperty m_ConvertToNormalMap;
        SerializedProperty m_PlatformSettingsArrProp;

        SerializedProperty m_ImportHiddenLayers;
        SerializedProperty m_LayerImportMode;
        SerializedProperty m_DefaultPivotSpace;
        SerializedProperty m_DefaultPivotAlignment;
        SerializedProperty m_CustomPivotPosition;

        SerializedProperty m_GenerateModelPrefab;
        SerializedProperty m_AddShadowCasters;
        SerializedProperty m_GenerateAnimationClips;
        SerializedProperty m_PrevGenerateAnimationClips;

        VisualElement m_RootVisualElement;
        VisualElement m_InspectorSettingsView;
        IMGUIContainer m_ToolbarContainer;
        
        readonly Dictionary<TextureImporterType, Action[]> m_AdvancedInspectorGUI = new Dictionary<TextureImporterType, Action[]>();
        bool m_IsPOT = false;
        readonly AsepriteImporterEditorFoldOutState m_EditorFoldOutState = new AsepriteImporterEditorFoldOutState();
        bool m_ShowPerAxisWrapModes = false;
        readonly int[] m_FilterModeOptions = (int[])(Enum.GetValues(typeof(FilterMode)));
        TexturePlatformSettingsHelper m_TexturePlatformSettingsHelper;
        InspectorGUI[] m_InspectorUI;
        int m_ActiveEditorIndex = 0;
        
        bool ITexturePlatformSettingsDataProvider.textureTypeHasMultipleDifferentValues => m_TextureType.hasMultipleDifferentValues;
        TextureImporterType ITexturePlatformSettingsDataProvider.textureType => (TextureImporterType)m_TextureType.intValue;

        SpriteImportMode ITexturePlatformSettingsDataProvider.spriteImportMode => spriteImportMode;
        SpriteImportMode spriteImportMode => (SpriteImportMode)m_SpriteMode.intValue;

        AnimationClip m_DefaultClip;
        ModelPreviewer m_ModelPreviewer;
        AsepriteImporter[] m_ImporterTargets;

        /// <summary>
        /// The SerializedProperty of an array of TextureImporterPlatformSettings. 
        /// </summary>
        public SerializedProperty platformSettingsArray => m_PlatformSettingsArrProp;

        /// <summary>
        /// Implementation of AssetImporterEditor.OnEnable
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();

            m_ImporterTargets = new AsepriteImporter[targets.Length];
            for (var i = 0; i < targets.Length; ++i)
                m_ImporterTargets[i] = (AsepriteImporter)targets[i];
            
            var textureImporterSettingsSP = serializedObject.FindProperty("m_TextureImporterSettings");
            m_TextureType = textureImporterSettingsSP.FindPropertyRelative("m_TextureType");
            m_TextureShape = textureImporterSettingsSP.FindPropertyRelative("m_TextureShape");
            m_ConvertToNormalMap = textureImporterSettingsSP.FindPropertyRelative("m_ConvertToNormalMap");
            m_SpriteMode = textureImporterSettingsSP.FindPropertyRelative("m_SpriteMode");
            m_SpritePixelsToUnits = textureImporterSettingsSP.FindPropertyRelative("m_SpritePixelsToUnits");
            m_SpriteMeshType = textureImporterSettingsSP.FindPropertyRelative("m_SpriteMeshType");
            m_SpriteExtrude = textureImporterSettingsSP.FindPropertyRelative("m_SpriteExtrude");
            m_Alignment = textureImporterSettingsSP.FindPropertyRelative("m_Alignment");
            m_SpritePivot = textureImporterSettingsSP.FindPropertyRelative("m_SpritePivot");
            m_NPOTScale = textureImporterSettingsSP.FindPropertyRelative("m_NPOTScale");
            m_IsReadable = textureImporterSettingsSP.FindPropertyRelative("m_IsReadable");
            m_sRGBTexture = textureImporterSettingsSP.FindPropertyRelative("m_sRGBTexture");
            m_AlphaSource = textureImporterSettingsSP.FindPropertyRelative("m_AlphaSource");
#if ENABLE_TEXTURE_STREAMING
            m_StreamingMipmaps = textureImporterSettingsSP.FindPropertyRelative("m_StreamingMipmaps");
            m_StreamingMipmapsPriority = textureImporterSettingsSP.FindPropertyRelative("m_StreamingMipmapsPriority");
#endif
            m_MipMapMode = textureImporterSettingsSP.FindPropertyRelative("m_MipMapMode");
            m_EnableMipMap = textureImporterSettingsSP.FindPropertyRelative("m_EnableMipMap");
            m_FadeOut = textureImporterSettingsSP.FindPropertyRelative("m_FadeOut");
            m_BorderMipMap = textureImporterSettingsSP.FindPropertyRelative("m_BorderMipMap");
            m_MipMapsPreserveCoverage = textureImporterSettingsSP.FindPropertyRelative("m_MipMapsPreserveCoverage");
            m_AlphaTestReferenceValue = textureImporterSettingsSP.FindPropertyRelative("m_AlphaTestReferenceValue");
            m_MipMapFadeDistanceStart = textureImporterSettingsSP.FindPropertyRelative("m_MipMapFadeDistanceStart");
            m_MipMapFadeDistanceEnd = textureImporterSettingsSP.FindPropertyRelative("m_MipMapFadeDistanceEnd");
            m_AlphaIsTransparency = textureImporterSettingsSP.FindPropertyRelative("m_AlphaIsTransparency");
            m_FilterMode = textureImporterSettingsSP.FindPropertyRelative("m_FilterMode");
            m_Aniso = textureImporterSettingsSP.FindPropertyRelative("m_Aniso");
            m_WrapU = textureImporterSettingsSP.FindPropertyRelative("m_WrapU");
            m_WrapV = textureImporterSettingsSP.FindPropertyRelative("m_WrapV");
            m_WrapW = textureImporterSettingsSP.FindPropertyRelative("m_WrapW");   
            m_PlatformSettingsArrProp = extraDataSerializedObject.FindProperty("platformSettings");
            
            var asepriteImporterSettings = serializedObject.FindProperty("m_AsepriteImporterSettings");
            m_ImportHiddenLayers = asepriteImporterSettings.FindPropertyRelative("m_ImportHiddenLayers");
            m_LayerImportMode = asepriteImporterSettings.FindPropertyRelative("m_LayerImportMode");
            m_DefaultPivotSpace = asepriteImporterSettings.FindPropertyRelative("m_DefaultPivotSpace");
            m_DefaultPivotAlignment = asepriteImporterSettings.FindPropertyRelative("m_DefaultPivotAlignment");
            m_CustomPivotPosition = asepriteImporterSettings.FindPropertyRelative("m_CustomPivotPosition");

            m_GenerateModelPrefab = asepriteImporterSettings.FindPropertyRelative("m_GenerateModelPrefab");
            m_AddShadowCasters = asepriteImporterSettings.FindPropertyRelative("m_AddShadowCasters");
            m_GenerateAnimationClips = asepriteImporterSettings.FindPropertyRelative("m_GenerateAnimationClips");
            
            var prevAsepriteImporterSettings = serializedObject.FindProperty("m_PreviousAsepriteImporterSettings");
            m_PrevGenerateAnimationClips = prevAsepriteImporterSettings.FindPropertyRelative("m_GenerateAnimationClips");

            foreach (var t in targets)
            {
                m_IsPOT &= ((AsepriteImporter)t).isNPOT;
            }            
            
            m_TexturePlatformSettingsHelper = new TexturePlatformSettingsHelper(this);
            
            var advancedGUIAction = new Action[]
            {
                ColorSpaceGUI,
                AlphaHandlingGUI,
                POTScaleGUI,
                ReadableGUI,
                MipMapGUI
            };
            m_AdvancedInspectorGUI.Add(TextureImporterType.Sprite, advancedGUIAction);

            advancedGUIAction = new Action[]
            {
                POTScaleGUI,
                ReadableGUI,
                MipMapGUI
            };
            m_AdvancedInspectorGUI.Add(TextureImporterType.Default, advancedGUIAction);    
            
            m_InspectorUI = new []
            {
                new InspectorGUI()
                {
                    container = new IMGUIContainer(DoInspectorSettings)
                    {
                        name = "DoSettingsUI"  
                    },
                    needsRepaint = false
                }
            };     
            m_ActiveEditorIndex = Mathf.Max(EditorPrefs.GetInt(this.GetType().Name + "ActiveEditorIndex", 0), 0);
            m_ActiveEditorIndex %= m_InspectorUI.Length;

            InitPreview();
        }
        
        void InitPreview()
        {
            var t = (AsepriteImporter)target;
            var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(t.assetPath);

            if (m_ModelPreviewer != null)
            {
                m_ModelPreviewer.Dispose();
                m_ModelPreviewer = null;
            }

            if (gameObject != null)
            {
                var clips = GetAllAnimationClips(t.assetPath);
                m_ModelPreviewer = new ModelPreviewer(gameObject, clips);
                m_DefaultClip = clips != null && clips.Length > 0 ? clips[0] : null;
            }
        }

        static AnimationClip[] GetAllAnimationClips(string assetPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var clips = new List<AnimationClip>();
            for (var i = 0; i < assets.Length; ++i)
            {
                if (assets[i] is AnimationClip clip)
                    clips.Add(clip);
            }

            return clips.ToArray();
        }
        
        /// <summary>
        /// Override for AssetImporter.extraDataType
        /// </summary>
        protected override Type extraDataType => typeof(AsepriteImporterEditorExternalData);
        
        /// <summary>
        /// Override for AssetImporter.InitializeExtraDataInstance
        /// </summary>
        /// <param name="extraTarget">Target object</param>
        /// <param name="targetIndex">Target index</param>
        protected override void InitializeExtraDataInstance(UnityEngine.Object extraTarget, int targetIndex)
        {
            var importer = targets[targetIndex] as AsepriteImporter;
            var extraData = extraTarget as AsepriteImporterEditorExternalData;
            var platformSettingsNeeded = TexturePlatformSettingsHelper.PlatformSettingsNeeded(this);
            if (importer != null)
            {
                extraData.Init(importer, platformSettingsNeeded);
            }
        }
        
        /// <summary>
        /// Implementation of virtual method CreateInspectorGUI.
        /// </summary>
        /// <returns>VisualElement container for Inspector visual.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            var styleSheet = EditorGUIUtility.Load("packages/com.unity.2d.aseprite/Editor/Assets/UI/AsepriteImporterStyleSheet.uss") as StyleSheet;
            m_RootVisualElement = new VisualElement()
            {
                name = "Root"
            };
            if(EditorGUIUtility.isProSkin)
                m_RootVisualElement.AddToClassList("asepriteImporter-editor-dark");
            else
                m_RootVisualElement.AddToClassList("asepriteImporter-editor-light");
            m_RootVisualElement.styleSheets.Add(styleSheet);
            
            m_InspectorSettingsView = new VisualElement()
            {
                name = "InspectorSettings"
            };
            m_RootVisualElement.Add(m_InspectorSettingsView);
            m_RootVisualElement.schedule.Execute(VisualElementUpdate);
            
            ShowInspectorTab(m_ActiveEditorIndex);
            return m_RootVisualElement;
        }   
        
        /// <summary>
        /// Implementation of AssetImporterEditor.OnDisable
        /// </summary>
        public override void OnDisable()
        {
            base.OnDisable();

            if (m_ModelPreviewer != null)
            {
                m_ModelPreviewer.Dispose();
                m_ModelPreviewer = null;
            }
            
            if(m_RootVisualElement != null)
                m_RootVisualElement.Clear();
        }

        void ShowInspectorTab(int tab)
        {
            m_InspectorSettingsView.Clear();
            m_InspectorSettingsView.Add(m_InspectorUI[tab].container);
            m_InspectorUI[tab].onUIActivated?.Invoke();
        }        

        /// <summary>
        /// Override from AssetImporterEditor.RequiresConstantRepaint
        /// </summary>
        /// <returns>Returns true when in Layer Management tab for UI feedback update, false otherwise.</returns>
        public override bool RequiresConstantRepaint()
        {
            return m_InspectorUI[m_ActiveEditorIndex].needsRepaint;
        }

        void VisualElementUpdate()
        {
            serializedObject.Update();
            extraDataSerializedObject.Update();
            try
            {
                if(m_InspectorUI[m_ActiveEditorIndex].onUpdate != null)
                    m_InspectorUI[m_ActiveEditorIndex].onUpdate.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log("Update:"+e);
            }

            serializedObject.ApplyModifiedProperties();
            extraDataSerializedObject.ApplyModifiedProperties();
            m_RootVisualElement.schedule.Execute(VisualElementUpdate);
        }

        void DoInspectorSettings()
        {
            serializedObject.Update();
            extraDataSerializedObject.Update();
            DoSettingsUI();
            ApplyRevertGUIVisualElement();
        }        

        void DoSettingsUI()
        {
            if (m_EditorFoldOutState.DoGeneralUI(styles.generalHeaderText))
            {
                DoSpriteTextureTypeInspector();
                GUILayout.Space(5);
            }
            
            DoLayerImportInspector();
            DoGenerateAssetInspector();
            CommonTextureSettingsGUI();
            DoPlatformSettings();
            DoAdvancedInspector();
        }  
        
        void DoSpriteTextureTypeInspector()
        {
            using (new EditorGUI.DisabledScope(m_SpriteMode.intValue == 0))
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(m_SpritePixelsToUnits, styles.spritePixelsPerUnit);

                if (m_SpriteMode.intValue != (int)SpriteImportMode.Polygon && !m_SpriteMode.hasMultipleDifferentValues)
                {
                    EditorGUILayout.IntPopup(m_SpriteMeshType, styles.spriteMeshTypeOptions, new[] { 0, 1 }, styles.spriteMeshType);
                }
                
                EditorGUI.indentLevel--;
            }
            DoOpenSpriteEditorButton();
        }  
        
        void DoOpenSpriteEditorButton()
        {
            using (new EditorGUI.DisabledScope(targets.Length != 1))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(styles.spriteEditorButtonLabel))
                {
                    if (HasModified())
                    {
                        // To ensure Sprite Editor Window to have the latest texture import setting,
                        // We must applied those modified values first.
                        var dialogText = string.Format(s_Styles.unappliedSettingsDialogContent.text, ((AssetImporter)target).assetPath);
                        if (EditorUtility.DisplayDialog(s_Styles.unappliedSettingsDialogTitle.text,
                                dialogText, s_Styles.applyButtonLabel.text, s_Styles.cancelButtonLabel.text))
                        {
#if UNITY_2022_2_OR_NEWER
                            SaveChanges();
#else
                            ApplyAndImport();
#endif
                            InternalEditorBridge.ShowSpriteEditorWindow(this.assetTarget);

                            // We re-imported the asset which destroyed the editor, so we can't keep running the UI here.
                            GUIUtility.ExitGUI();
                        }
                    }
                    else
                    {
                        InternalEditorBridge.ShowSpriteEditorWindow(this.assetTarget);
                    }
                }
                GUILayout.EndHorizontal();
            }    
        }

#if UNITY_2022_2_OR_NEWER
        /// <summary>
        /// Implementation of AssetImporterEditor.SaveChanges.
        /// </summary>
        public override void SaveChanges()
        {
            ApplyTexturePlatformSettings();
            
            serializedObject.ApplyModifiedProperties();
            extraDataSerializedObject.ApplyModifiedProperties();
            base.SaveChanges();
        }        
#endif        

        void DoLayerImportInspector()
        {
            if ((TextureImporterType) m_TextureType.intValue != TextureImporterType.Sprite)
                return;
            
            if (m_EditorFoldOutState.DoLayerImportUI(styles.layerImportHeaderText))
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(m_ImportHiddenLayers, styles.importHiddenLayer);
                DrawLayerImportModes();
                EditorGUILayout.PropertyField(m_DefaultPivotSpace, styles.defaultPivotSpace);
                DrawPivotAlignment();

                if ((SpriteAlignment) m_DefaultPivotAlignment.intValue == SpriteAlignment.Custom)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    EditorGUILayout.PropertyField(m_CustomPivotPosition, new GUIContent());
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.Space(5);
                
                EditorGUI.indentLevel--;
            }
        }

        void DrawLayerImportModes()
        {
            EditorGUI.showMixedValue = m_LayerImportMode.hasMultipleDifferentValues;
            m_LayerImportMode.intValue = EditorGUILayout.IntPopup(s_Styles.layerImportMode, m_LayerImportMode.intValue, s_Styles.layerImportOptions, s_Styles.layerImportValues);
            EditorGUI.showMixedValue = false;
        }      
        
        void DrawPivotAlignment()
        {
            EditorGUI.showMixedValue = m_LayerImportMode.hasMultipleDifferentValues;
            m_DefaultPivotAlignment.intValue = EditorGUILayout.IntPopup(s_Styles.defaultPivotAlignment, m_DefaultPivotAlignment.intValue, s_Styles.spriteAlignmentOptions, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            EditorGUI.showMixedValue = false;
        }
        
        void DoGenerateAssetInspector()
        {
            if ((TextureImporterType) m_TextureType.intValue != TextureImporterType.Sprite)
                return;

#if ENABLE_URP
            var isUrpEnabled = true;
#else
            var isUrpEnabled = false;
#endif             
            
            if (m_EditorFoldOutState.DoGenerateAssetUI(styles.generateAssetsHeaderText))
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(m_GenerateModelPrefab, styles.generateModelPrefab);
                EditorGUI.indentLevel++;

#if UNITY_2023_1_OR_NEWER
                var isDisabled = !(isUrpEnabled && m_GenerateModelPrefab.boolValue);
                using (new EditorGUI.DisabledScope(isDisabled))
                {
                    EditorGUILayout.PropertyField(m_AddShadowCasters, styles.addShadowCasters);    
                }
#endif
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.PropertyField(m_GenerateAnimationClips, styles.generateAnimationClips);
                ExportAnimationAssetsButton();
                
                GUILayout.Space(5);
                
                EditorGUI.indentLevel--;
            }
        }

        void ExportAnimationAssetsButton()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(targets.Length > 1 || 
                                               m_DefaultClip == null ||
                                               !m_GenerateAnimationClips.boolValue))
            {
                if (GUILayout.Button(styles.exportAnimationAssetsText))
                {
                    ImportUtilities.ExportAnimationAssets(m_ImporterTargets);
                    Apply();
                    GUIUtility.ExitGUI();
                }
            }
            
            GUILayout.EndHorizontal();  
            
            // If the Generate Animation Clips checkbox has been checked, but not yet applied.
            if (m_GenerateAnimationClips.boolValue && !m_PrevGenerateAnimationClips.boolValue)
            {
                EditorGUILayout.HelpBox(styles.exportAnimationInfoText.text, MessageType.Info);   
            }
        }

        void CommonTextureSettingsGUI()
        {
            if (m_EditorFoldOutState.DoTextureUI(styles.textureHeaderText))
            {
                EditorGUI.indentLevel++;
                
                EditorGUI.BeginChangeCheck();

                // Wrap mode
                var isVolume = false;
                WrapModePopup(m_WrapU, m_WrapV, m_WrapW, isVolume, ref m_ShowPerAxisWrapModes);
                
                // Display warning about repeat wrap mode on restricted npot emulation
                if (m_NPOTScale.intValue == (int)TextureImporterNPOTScale.None &&
                    (m_WrapU.intValue == (int)TextureWrapMode.Repeat || m_WrapV.intValue == (int)TextureWrapMode.Repeat) &&
                    !InternalEditorBridge.DoesHardwareSupportsFullNPOT())
                {
                    var displayWarning = false;
                    foreach (var target in targets)
                    {
                        var imp = (AsepriteImporter)target;
                        var w = imp.textureActualWidth;
                        var h = imp.textureActualHeight;
                        if (!Mathf.IsPowerOfTwo(w) || !Mathf.IsPowerOfTwo(h))
                        {
                            displayWarning = true;
                            break;
                        }
                    }

                    if (displayWarning)
                    {
                        EditorGUILayout.HelpBox(styles.warpNotSupportWarning.text, MessageType.Warning, true);
                    }
                }

                // Filter mode
                EditorGUI.showMixedValue = m_FilterMode.hasMultipleDifferentValues;
                var filter = (FilterMode)m_FilterMode.intValue;
                if ((int)filter == -1)
                {
                    if (m_FadeOut.intValue > 0 || m_ConvertToNormalMap.intValue > 0)
                        filter = FilterMode.Trilinear;
                    else
                        filter = FilterMode.Bilinear;
                }
                filter = (FilterMode)EditorGUILayout.IntPopup(styles.filterMode, (int)filter, styles.filterModeOptions, m_FilterModeOptions);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                    m_FilterMode.intValue = (int)filter;

                // Aniso
                var showAniso = (FilterMode)m_FilterMode.intValue != FilterMode.Point
                    && m_EnableMipMap.intValue > 0
                    && (TextureImporterShape)m_TextureShape.intValue != TextureImporterShape.TextureCube;
                using (new EditorGUI.DisabledScope(!showAniso))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = m_Aniso.hasMultipleDifferentValues;
                    int aniso = m_Aniso.intValue;
                    if (aniso == -1)
                        aniso = 1;
                    aniso = EditorGUILayout.IntSlider(styles.anisoLevelLabel, aniso, 0, 16);
                    EditorGUI.showMixedValue = false;
                    if (EditorGUI.EndChangeCheck())
                        m_Aniso.intValue = aniso;

                    if (aniso > 1)
                    {
                        if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.Disable)
                            EditorGUILayout.HelpBox(styles.anisotropicDisableInfo.text, MessageType.Info);
                        else if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable)
                            EditorGUILayout.HelpBox(styles.anisotropicForceEnableInfo.text, MessageType.Info);
                    }
                }
                GUILayout.Space(5);
                
                EditorGUI.indentLevel--;
            }
        }    
        
        // showPerAxisWrapModes is state of whether "Per-Axis" mode should be active in the main dropdown.
        // It is set automatically if wrap modes in UVW are different, or if user explicitly picks "Per-Axis" option -- when that one is picked,
        // then it should stay true even if UVW wrap modes will initially be the same.
        //
        // Note: W wrapping mode is only shown when isVolumeTexture is true.
        static void WrapModePopup(SerializedProperty wrapU, SerializedProperty wrapV, SerializedProperty wrapW, bool isVolumeTexture, ref bool showPerAxisWrapModes)
        {
            // In texture importer settings, serialized properties for things like wrap modes can contain -1;
            // that seems to indicate "use defaults, user has not changed them to anything" but not totally sure.
            // Show them as Repeat wrap modes in the popups.
            var wu = (TextureWrapMode)Mathf.Max(wrapU.intValue, 0);
            var wv = (TextureWrapMode)Mathf.Max(wrapV.intValue, 0);
            var ww = (TextureWrapMode)Mathf.Max(wrapW.intValue, 0);

            // automatically go into per-axis mode if values are already different
            if (wu != wv)
                showPerAxisWrapModes = true;
            if (isVolumeTexture)
            {
                if (wu != ww || wv != ww)
                    showPerAxisWrapModes = true;
            }

            // It's not possible to determine whether any single texture in the whole selection is using per-axis wrap modes
            // just from SerializedProperty values. They can only tell if "some values in whole selection are different" (e.g.
            // wrap value on U axis is not the same among all textures), and can return value of "some" object in the selection
            // (typically based on object loading order). So in order for more intuitive behavior with multi-selection,
            // we go over the actual objects when there's >1 object selected and some wrap modes are different.
            if (!showPerAxisWrapModes)
            {
                if (wrapU.hasMultipleDifferentValues || wrapV.hasMultipleDifferentValues || (isVolumeTexture && wrapW.hasMultipleDifferentValues))
                {
                    if (IsAnyTextureObjectUsingPerAxisWrapMode(wrapU.serializedObject.targetObjects, isVolumeTexture))
                    {
                        showPerAxisWrapModes = true;
                    }
                }
            }

            int value = showPerAxisWrapModes ? -1 : (int)wu;

            // main wrap mode popup
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = !showPerAxisWrapModes && (wrapU.hasMultipleDifferentValues || wrapV.hasMultipleDifferentValues || (isVolumeTexture && wrapW.hasMultipleDifferentValues));
            value = EditorGUILayout.IntPopup(styles.wrapModeLabel, value, styles.wrapModeContents, styles.wrapModeValues);
            if (EditorGUI.EndChangeCheck() && value != -1)
            {
                // assign the same wrap mode to all axes, and hide per-axis popups
                wrapU.intValue = value;
                wrapV.intValue = value;
                wrapW.intValue = value;
                showPerAxisWrapModes = false;
            }

            // show per-axis popups if needed
            if (value == -1)
            {
                showPerAxisWrapModes = true;
                EditorGUI.indentLevel++;
                WrapModeAxisPopup(styles.wrapU, wrapU);
                WrapModeAxisPopup(styles.wrapV, wrapV);
                if (isVolumeTexture)
                {
                    WrapModeAxisPopup(styles.wrapW, wrapW);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.showMixedValue = false;
        }

        static void WrapModeAxisPopup(GUIContent label, SerializedProperty wrapProperty)
        {
            // In texture importer settings, serialized properties for wrap modes can contain -1, which means "use default".
            var wrap = (TextureWrapMode)Mathf.Max(wrapProperty.intValue, 0);
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(rect, label, wrapProperty);
            wrap = (TextureWrapMode)EditorGUI.EnumPopup(rect, label, wrap);
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                wrapProperty.intValue = (int)wrap;
            }
        }     
        
        static bool IsAnyTextureObjectUsingPerAxisWrapMode(UnityEngine.Object[] objects, bool isVolumeTexture)
        {
            foreach (var o in objects)
            {
                int u = 0, v = 0, w = 0;
                // the objects can be Textures themselves, or texture-related importers
                if (o is Texture)
                {
                    var ti = (Texture)o;
                    u = (int)ti.wrapModeU;
                    v = (int)ti.wrapModeV;
                    w = (int)ti.wrapModeW;
                }
                if (o is TextureImporter)
                {
                    var ti = (TextureImporter)o;
                    u = (int)ti.wrapModeU;
                    v = (int)ti.wrapModeV;
                    w = (int)ti.wrapModeW;
                }
                if (o is IHVImageFormatImporter)
                {
                    var ti = (IHVImageFormatImporter)o;
                    u = (int)ti.wrapModeU;
                    v = (int)ti.wrapModeV;
                    w = (int)ti.wrapModeW;
                }
                u = Mathf.Max(0, u);
                v = Mathf.Max(0, v);
                w = Mathf.Max(0, w);
                if (u != v)
                {
                    return true;
                }
                if (isVolumeTexture)
                {
                    if (u != w || v != w)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void DoPlatformSettings()
        {
            if (m_EditorFoldOutState.DoPlatformSettingsUI(styles.platformSettingsHeaderText))
            {
                GUILayout.Space(5);
                m_TexturePlatformSettingsHelper.ShowPlatformSpecificSettings();
                GUILayout.Space(5);
            }
        }
        
        void DoAdvancedInspector()
        {
            if (m_TextureType.hasMultipleDifferentValues)
                return;
            
            if (m_AdvancedInspectorGUI.ContainsKey((TextureImporterType)m_TextureType.intValue))
            {
                if (m_EditorFoldOutState.DoAdvancedUI(styles.advancedHeaderText))
                {
                    foreach (var action in m_AdvancedInspectorGUI[(TextureImporterType) m_TextureType.intValue])
                    {
                        EditorGUI.indentLevel++;
                        action();
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }        
        
        void ColorSpaceGUI()
        {
            ToggleFromInt(m_sRGBTexture, styles.sRGBTexture);
        }
        
        void AlphaHandlingGUI()
        {
            EditorGUI.showMixedValue = m_AlphaSource.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int newAlphaUsage = EditorGUILayout.IntPopup(styles.alphaSource, m_AlphaSource.intValue, styles.alphaSourceOptions, styles.alphaSourceValues);

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                m_AlphaSource.intValue = newAlphaUsage;
            }

            bool showAlphaIsTransparency = (TextureImporterAlphaSource)m_AlphaSource.intValue != TextureImporterAlphaSource.None;
            using (new EditorGUI.DisabledScope(!showAlphaIsTransparency))
            {
                ToggleFromInt(m_AlphaIsTransparency, styles.alphaIsTransparency);
            }
        }
        
        void POTScaleGUI()
        {
            using (new EditorGUI.DisabledScope(m_IsPOT || m_TextureType.intValue == (int)TextureImporterType.Sprite))
            {
                EnumPopup(m_NPOTScale, typeof(TextureImporterNPOTScale), styles.npot);
            }
        }      
        
        void ReadableGUI()
        {
            ToggleFromInt(m_IsReadable, styles.readWrite);
        }        

        void MipMapGUI()
        {
            ToggleFromInt(m_EnableMipMap, styles.generateMipMaps);

            if (m_EnableMipMap.boolValue && !m_EnableMipMap.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;
                ToggleFromInt(m_BorderMipMap, styles.borderMipMaps);

#if ENABLE_TEXTURE_STREAMING                
                ToggleFromInt(m_StreamingMipmaps, styles.streamingMipMaps);
                if (m_StreamingMipmaps.boolValue && !m_StreamingMipmaps.hasMultipleDifferentValues)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_StreamingMipmapsPriority, styles.streamingMipmapsPriority);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_StreamingMipmapsPriority.intValue = Mathf.Clamp(m_StreamingMipmapsPriority.intValue, -128, 127);
                    }
                    EditorGUI.indentLevel--;
                }
#endif                
                
                m_MipMapMode.intValue = EditorGUILayout.Popup(styles.mipMapFilter, m_MipMapMode.intValue, styles.mipMapFilterOptions);

                ToggleFromInt(m_MipMapsPreserveCoverage, styles.mipMapsPreserveCoverage);
                if (m_MipMapsPreserveCoverage.intValue != 0 && !m_MipMapsPreserveCoverage.hasMultipleDifferentValues)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_AlphaTestReferenceValue, styles.alphaTestReferenceValue);
                    EditorGUI.indentLevel--;
                }

                // Mipmap fadeout
                ToggleFromInt(m_FadeOut, styles.mipmapFadeOutToggle);
                if (m_FadeOut.intValue > 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    float min = m_MipMapFadeDistanceStart.intValue;
                    float max = m_MipMapFadeDistanceEnd.intValue;
                    EditorGUILayout.MinMaxSlider(styles.mipmapFadeOut, ref min, ref max, 0, 10);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MipMapFadeDistanceStart.intValue = Mathf.RoundToInt(min);
                        m_MipMapFadeDistanceEnd.intValue = Mathf.RoundToInt(max);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        void ApplyRevertGUIVisualElement()
        {
            serializedObject.ApplyModifiedProperties();
            extraDataSerializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        /// <summary>
        /// Implementation of AssetImporterEditor.Apply
        /// </summary>
        protected override void Apply()
        {
            InternalEditorBridge.ApplySpriteEditorWindow();
            base.Apply();
            
            if (m_ModelPreviewer != null)
            {
                m_ModelPreviewer.Dispose();
                m_ModelPreviewer = null;
            }
        }

        void ApplyTexturePlatformSettings()
        {
            for(var i = 0; i< targets.Length && i < extraDataTargets.Length; ++i)
            {
                var asepriteImporter = (AsepriteImporter)targets[i];
                var externalData = (AsepriteImporterEditorExternalData)extraDataTargets[i];
                foreach (var ps in externalData.platformSettings)
                {
                    asepriteImporter.SetImporterPlatformSettings(ps);
                }
            }
        }

        /// <summary>
        /// Override of AssetImporterEditor.HasModified.
        /// </summary>
        /// <returns>Returns True if has modified data. False otherwise.</returns>
        public override bool HasModified()
        {
            if (base.HasModified())
                return true;

            return m_TexturePlatformSettingsHelper.HasModified();
        }        

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.GetTargetCount.
        /// </summary>
        /// <returns>Returns the number of selected targets.</returns>
        int ITexturePlatformSettingsDataProvider.GetTargetCount()
        {
            return targets.Length;
        }

        /// <summary>
        /// ITexturePlatformSettingsDataProvider.GetPlatformTextureSettings.
        /// </summary>
        /// <param name="i">Selected target index.</param>
        /// <param name="name">Name of the platform.</param>
        /// <returns>TextureImporterPlatformSettings for the given platform name and selected target index.</returns>
        TextureImporterPlatformSettings ITexturePlatformSettingsDataProvider.GetPlatformTextureSettings(int i, string name)
        {
            var externalData = extraDataSerializedObject.targetObjects[i] as AsepriteImporterEditorExternalData;
            if (externalData != null)
            {
                foreach (var ps in externalData.platformSettings)
                {
                    if (ps.name == name)
                        return ps;
                }
            }
            return new TextureImporterPlatformSettings()
            {
                name = name,
                overridden = false
            };
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.ShowPresetSettings.
        /// </summary>
        /// <returns>True if valid asset is selected, false otherwise.</returns>
        bool ITexturePlatformSettingsDataProvider.ShowPresetSettings()
        {
            return assetTarget == null;
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.DoesSourceTextureHaveAlpha.
        /// </summary>
        /// <param name="i">Index to selected target.</param>
        /// <returns>Always returns true since importer deals with source file that has alpha.</returns>
        bool ITexturePlatformSettingsDataProvider.DoesSourceTextureHaveAlpha(int i)
        {
            return true;
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.IsSourceTextureHDR.
        /// </summary>
        /// <param name="i">Index to selected target.</param>
        /// <returns>Always returns false since importer does not handle HDR textures.</returns>
        bool ITexturePlatformSettingsDataProvider.IsSourceTextureHDR(int i)
        {
            return false;
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.SetPlatformTextureSettings.
        /// </summary>
        /// <param name="i">Selected target index.</param>
        /// <param name="platformSettings">TextureImporterPlatformSettings to apply to target.</param>
        void ITexturePlatformSettingsDataProvider.SetPlatformTextureSettings(int i, TextureImporterPlatformSettings platformSettings)
        {
            var psdImporter = ((AsepriteImporter)targets[i]);
            var sp = new SerializedObject(psdImporter);
            sp.FindProperty("m_PlatformSettingsDirtyTick").longValue = System.DateTime.Now.Ticks;
            sp.ApplyModifiedProperties();
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.GetImporterSettings.
        /// </summary>
        /// <param name="i">Selected target index.</param>
        /// <param name="settings">TextureImporterPlatformSettings reference for data retrieval.</param>
        void ITexturePlatformSettingsDataProvider.GetImporterSettings(int i, TextureImporterSettings settings)
        {
            ((AsepriteImporter)targets[i]).ReadTextureSettings(settings);
            // Get settings that have been changed in the inspector
            GetSerializedPropertySettings(settings);
        }

        /// <summary>
        /// Get the name property of TextureImporterPlatformSettings from a SerializedProperty.
        /// </summary>
        /// <param name="sp">The SerializedProperty to retrive data.</param>
        /// <returns>The name value in string.</returns>
        public string GetBuildTargetName(SerializedProperty sp)
        {
            return sp.FindPropertyRelative("m_Name").stringValue;
        }
        
        TextureImporterSettings GetSerializedPropertySettings(TextureImporterSettings settings)
        {
            if (!m_AlphaSource.hasMultipleDifferentValues)
                settings.alphaSource = (TextureImporterAlphaSource)m_AlphaSource.intValue;

            if (!m_ConvertToNormalMap.hasMultipleDifferentValues)
                settings.convertToNormalMap = m_ConvertToNormalMap.intValue > 0;

            if (!m_BorderMipMap.hasMultipleDifferentValues)
                settings.borderMipmap = m_BorderMipMap.intValue > 0;

#if ENABLE_TEXTURE_STREAMING
            if (!m_StreamingMipmaps.hasMultipleDifferentValues)
                settings.streamingMipmaps = m_StreamingMipmaps.intValue > 0;
            if (!m_StreamingMipmapsPriority.hasMultipleDifferentValues)
                settings.streamingMipmapsPriority = m_StreamingMipmapsPriority.intValue;
#endif            

            if (!m_MipMapsPreserveCoverage.hasMultipleDifferentValues)
                settings.mipMapsPreserveCoverage = m_MipMapsPreserveCoverage.intValue > 0;

            if (!m_AlphaTestReferenceValue.hasMultipleDifferentValues)
                settings.alphaTestReferenceValue = m_AlphaTestReferenceValue.floatValue;

            if (!m_NPOTScale.hasMultipleDifferentValues)
                settings.npotScale = (TextureImporterNPOTScale)m_NPOTScale.intValue;

            if (!m_IsReadable.hasMultipleDifferentValues)
                settings.readable = m_IsReadable.intValue > 0;

            if (!m_sRGBTexture.hasMultipleDifferentValues)
                settings.sRGBTexture = m_sRGBTexture.intValue > 0;

            if (!m_EnableMipMap.hasMultipleDifferentValues)
                settings.mipmapEnabled = m_EnableMipMap.intValue > 0;

            if (!m_MipMapMode.hasMultipleDifferentValues)
                settings.mipmapFilter = (TextureImporterMipFilter)m_MipMapMode.intValue;

            if (!m_FadeOut.hasMultipleDifferentValues)
                settings.fadeOut = m_FadeOut.intValue > 0;

            if (!m_MipMapFadeDistanceStart.hasMultipleDifferentValues)
                settings.mipmapFadeDistanceStart = m_MipMapFadeDistanceStart.intValue;

            if (!m_MipMapFadeDistanceEnd.hasMultipleDifferentValues)
                settings.mipmapFadeDistanceEnd = m_MipMapFadeDistanceEnd.intValue;

            if (!m_SpriteMode.hasMultipleDifferentValues)
                settings.spriteMode = m_SpriteMode.intValue;

            if (!m_SpritePixelsToUnits.hasMultipleDifferentValues)
                settings.spritePixelsPerUnit = m_SpritePixelsToUnits.floatValue;

            if (!m_SpriteExtrude.hasMultipleDifferentValues)
                settings.spriteExtrude = (uint)m_SpriteExtrude.intValue;

            if (!m_SpriteMeshType.hasMultipleDifferentValues)
                settings.spriteMeshType = (SpriteMeshType)m_SpriteMeshType.intValue;

            if (!m_Alignment.hasMultipleDifferentValues)
                settings.spriteAlignment = m_Alignment.intValue;

            if (!m_SpritePivot.hasMultipleDifferentValues)
                settings.spritePivot = m_SpritePivot.vector2Value;

            if (!m_WrapU.hasMultipleDifferentValues)
                settings.wrapModeU = (TextureWrapMode)m_WrapU.intValue;
            if (!m_WrapV.hasMultipleDifferentValues)
                settings.wrapModeU = (TextureWrapMode)m_WrapV.intValue;
            if (!m_WrapW.hasMultipleDifferentValues)
                settings.wrapModeU = (TextureWrapMode)m_WrapW.intValue;

            if (!m_FilterMode.hasMultipleDifferentValues)
                settings.filterMode = (FilterMode)m_FilterMode.intValue;

            if (!m_Aniso.hasMultipleDifferentValues)
                settings.aniso = m_Aniso.intValue;


            if (!m_AlphaIsTransparency.hasMultipleDifferentValues)
                settings.alphaIsTransparency = m_AlphaIsTransparency.intValue > 0;

            if (!m_TextureType.hasMultipleDifferentValues)
                settings.textureType = (TextureImporterType)m_TextureType.intValue;

            if (!m_TextureShape.hasMultipleDifferentValues)
                settings.textureShape = (TextureImporterShape)m_TextureShape.intValue;

            return settings;
        }

        static void ToggleFromInt(SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            var value = EditorGUILayout.Toggle(label, property.intValue > 0) ? 1 : 0;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                property.intValue = value;
        }

        static void EnumPopup(SerializedProperty property, System.Type type, GUIContent label)
        {
            EditorGUILayout.IntPopup(label.text, property.intValue,
                System.Enum.GetNames(type),
                System.Enum.GetValues(type) as int[]);
        }

        /// <summary>
        /// Override from AssetImporterEditor to show custom preview.
        /// </summary>
        /// <param name="r">Preview Rect.</param>
        public override void DrawPreview(Rect r)
        {
            if (m_ModelPreviewer == null)
                InitPreview();

            if (m_ModelPreviewer != null)
                m_ModelPreviewer.DrawPreview(r, "PreBackgroundSolid");
            else
                base.DrawPreview(r);
        }        
        
        class Styles
        {
            public readonly GUIContent textureTypeTitle = new GUIContent("Texture Type", "What will this texture be used for?");
            public readonly GUIContent[] textureTypeOptions =
            {
                new GUIContent("Default", "Texture is a normal image such as a diffuse texture or other."),
                new GUIContent("Sprite (2D and UI)", "Texture is used for a sprite."),
            };
            public readonly int[] textureTypeValues =
            {
                (int)TextureImporterType.Default,
                (int)TextureImporterType.Sprite,
            };
            
            readonly GUIContent textureShape2D = new GUIContent("2D, Texture is 2D.");
            readonly  GUIContent textureShapeCube = new GUIContent("Cube", "Texture is a Cubemap.");
            public readonly Dictionary<TextureImporterShape, GUIContent[]> textureShapeOptionsDictionnary = new Dictionary<TextureImporterShape, GUIContent[]>();
            public readonly Dictionary<TextureImporterShape, int[]> textureShapeValuesDictionnary = new Dictionary<TextureImporterShape, int[]>();


            public readonly GUIContent filterMode = new GUIContent("Filter Mode");
            public readonly GUIContent[] filterModeOptions =
            {
                new GUIContent("Point (no filter)"),
                new GUIContent("Bilinear"),
                new GUIContent("Trilinear")
            };

            public readonly GUIContent mipmapFadeOutToggle = new GUIContent("Fadeout Mip Maps");
            public readonly GUIContent mipmapFadeOut = new GUIContent("Fade Range");
            public readonly GUIContent readWrite = new GUIContent("Read/Write Enabled", "Enable to be able to access the raw pixel data from code.");

            public readonly GUIContent alphaSource = new GUIContent("Alpha Source", "How is the alpha generated for the imported texture.");
            public readonly GUIContent[] alphaSourceOptions =
            {
                new GUIContent("None", "No Alpha will be used."),
                new GUIContent("Input Texture Alpha", "Use Alpha from the input texture if one is provided."),
                new GUIContent("From Gray Scale", "Generate Alpha from image gray scale."),
            };
            public readonly int[] alphaSourceValues =
            {
                (int)TextureImporterAlphaSource.None,
                (int)TextureImporterAlphaSource.FromInput,
                (int)TextureImporterAlphaSource.FromGrayScale,
            };

            public readonly GUIContent generateMipMaps = new GUIContent("Generate Mip Maps");
            public readonly GUIContent sRGBTexture = new GUIContent("sRGB (Color Texture)", "Texture content is stored in gamma space. Non-HDR color textures should enable this flag (except if used for IMGUI).");
            public readonly GUIContent borderMipMaps = new GUIContent("Border Mip Maps");
#if ENABLE_TEXTURE_STREAMING            
            public readonly GUIContent streamingMipMaps = EditorGUIUtility.TrTextContent("Mip Streaming", "Only load larger mipmaps as needed to render the current game cameras. Requires texture streaming to be enabled in quality settings.");
            public readonly GUIContent streamingMipmapsPriority = EditorGUIUtility.TrTextContent("Priority", "Mipmap streaming priority when there's contention for resources. Positive numbers represent higher priority. Valid range is -128 to 127.");
#endif            
            public readonly GUIContent mipMapsPreserveCoverage = new GUIContent("Mip Maps Preserve Coverage", "The alpha channel of generated Mip Maps will preserve coverage during the alpha test.");
            public readonly GUIContent alphaTestReferenceValue = new GUIContent("Alpha Cutoff Value", "The reference value used during the alpha test. Controls Mip Map coverage.");
            public readonly GUIContent mipMapFilter = new GUIContent("Mip Map Filtering");
            public readonly GUIContent[] mipMapFilterOptions =
            {
                new GUIContent("Box"),
                new GUIContent("Kaiser"),
            };
            public readonly GUIContent npot = new GUIContent("Non Power of 2", "How non-power-of-two textures are scaled on import.");
            
            public readonly GUIContent[] spriteMeshTypeOptions =
            {
                new GUIContent("Full Rect"),
                new GUIContent("Tight"),
            };
            
            public readonly GUIContent spritePixelsPerUnit = new GUIContent("Pixels Per Unit", "How many pixels in the sprite correspond to one unit in the world.");
            public readonly GUIContent spriteMeshType = new GUIContent("Mesh Type", "Type of sprite mesh to generate.");

            public readonly GUIContent warpNotSupportWarning = new GUIContent("Graphics device doesn't support Repeat wrap mode on NPOT textures. Falling back to Clamp.");
            public readonly GUIContent anisoLevelLabel = new GUIContent("Aniso Level");
            public readonly GUIContent anisotropicDisableInfo = new GUIContent("Anisotropic filtering is disabled for all textures in Quality Settings.");
            public readonly GUIContent anisotropicForceEnableInfo = new GUIContent("Anisotropic filtering is enabled for all textures in Quality Settings.");
            public readonly GUIContent unappliedSettingsDialogTitle = new GUIContent("Unapplied import settings");
            public readonly GUIContent unappliedSettingsDialogContent = new GUIContent("Unapplied import settings for \'{0}\'.\nApply and continue to sprite editor or cancel.");
            public readonly GUIContent applyButtonLabel = new GUIContent("Apply");
            public readonly GUIContent cancelButtonLabel = new GUIContent("Cancel");
            public readonly GUIContent spriteEditorButtonLabel = new GUIContent("Open Sprite Editor");
            public readonly GUIContent alphaIsTransparency = new GUIContent("Alpha Is Transparency", "If the provided alpha channel is transparency, enable this to pre-filter the color to avoid texture filtering artifacts. This is not supported for HDR textures.");
            
            public readonly GUIContent advancedHeaderText = new GUIContent("Advanced", "Show advanced settings.");

            public readonly GUIContent platformSettingsHeaderText  = new GUIContent("Platform Settings");

            public readonly GUIContent[] platformSettingsSelection;

            public readonly GUIContent wrapModeLabel = new GUIContent("Wrap Mode");
            public readonly GUIContent wrapU = new GUIContent("U axis");
            public readonly GUIContent wrapV = new GUIContent("V axis");
            public readonly GUIContent wrapW = new GUIContent("W axis");


            public readonly GUIContent[] wrapModeContents =
            {
                new GUIContent("Repeat"),
                new GUIContent("Clamp"),
                new GUIContent("Mirror"),
                new GUIContent("Mirror Once"),
                new GUIContent("Per-axis")
            };
            public readonly int[] wrapModeValues =
            {
                (int)TextureWrapMode.Repeat,
                (int)TextureWrapMode.Clamp,
                (int)TextureWrapMode.Mirror,
                (int)TextureWrapMode.MirrorOnce,
                -1
            };

            public readonly GUIContent importHiddenLayer = EditorGUIUtility.TrTextContent("Include Hidden Layers", "Settings to determine when hidden layers should be imported.");
            public readonly GUIContent defaultPivotSpace = EditorGUIUtility.TrTextContent("Pivot Space", "Select which space the pivot should be calculated in.");
            public readonly GUIContent defaultPivotAlignment = EditorGUIUtility.TrTextContent("Pivot Alignment", "Select where the pivot should be located based on the Pivot Space.");

            public readonly GUIContent[] spriteAlignmentOptions =
            {
                new GUIContent("Center"),
                new GUIContent("Top Left"),
                new GUIContent("Top"),
                new GUIContent("Top Right"),
                new GUIContent("Left"),
                new GUIContent("Right"),
                new GUIContent("Bottom Left"),
                new GUIContent("Bottom"),
                new GUIContent("Bottom Right"),
                new GUIContent("Custom"),
            };

            public readonly GUIContent layerImportMode = EditorGUIUtility.TrTextContent("Import Mode", "Choose between generating one Sprite per layer, or merge all layers in a frame into a single Sprite.");
            public readonly GUIContent[] layerImportOptions =
            {
                new GUIContent("Individual Layers", "Generate one Sprite per layer."),
                new GUIContent("Merged Layers", "Merge all layers in a frame into a single Sprite."),
            };

            public readonly int[] layerImportValues =
            {
                (int)LayerImportModes.Individual,
                (int)LayerImportModes.Merged
            };            
            
            public readonly GUIContent generateModelPrefab = EditorGUIUtility.TrTextContent("Model Prefab", "Generate a Model Prefab laid out the same way as inside Aseprite.");
            public readonly GUIContent addShadowCasters = EditorGUIUtility.TrTextContent("Shadow Casters", "Add Shadow Casters on all GameObjects with SpriteRenderer. Note: The Universal Rendering Pipeline package has to be installed.");
            public readonly GUIContent generateAnimationClips = EditorGUIUtility.TrTextContent("Animation Clips", "Generate Animation Clips based on the frame and tag data from the Aseprite file.");
            
            public readonly GUIContent generalHeaderText = EditorGUIUtility.TrTextContent("General", "General settings.");
            public readonly GUIContent layerImportHeaderText = EditorGUIUtility.TrTextContent("Layer Import","Layer Import settings.");
            public readonly GUIContent generateAssetsHeaderText = EditorGUIUtility.TrTextContent("Generate Assets","Generated assets settings.");
            public readonly GUIContent textureHeaderText = EditorGUIUtility.TrTextContent("Texture","Texture settings.");
            
            public readonly GUIContent exportAnimationAssetsText = EditorGUIUtility.TrTextContent("Export Animation Assets");
            public readonly GUIContent exportAnimationInfoText = EditorGUIUtility.TrTextContent("To enable the Export Animation Assets button, make sure to first Apply the changes.");

            public Styles()
            {
                // This is far from ideal, but it's better than having tons of logic in the GUI code itself.
                // The combination should not grow too much anyway since only Texture3D will be added later.
                GUIContent[] s2D_Options = { textureShape2D };
                GUIContent[] sCube_Options = { textureShapeCube };
                GUIContent[] s2D_Cube_Options = { textureShape2D, textureShapeCube };
                textureShapeOptionsDictionnary.Add(TextureImporterShape.Texture2D, s2D_Options);
                textureShapeOptionsDictionnary.Add(TextureImporterShape.TextureCube, sCube_Options);
                textureShapeOptionsDictionnary.Add(TextureImporterShape.Texture2D | TextureImporterShape.TextureCube, s2D_Cube_Options);

                int[] s2D_Values = { (int)TextureImporterShape.Texture2D };
                int[] sCube_Values = { (int)TextureImporterShape.TextureCube };
                int[] s2D_Cube_Values = { (int)TextureImporterShape.Texture2D, (int)TextureImporterShape.TextureCube };
                textureShapeValuesDictionnary.Add(TextureImporterShape.Texture2D, s2D_Values);
                textureShapeValuesDictionnary.Add(TextureImporterShape.TextureCube, sCube_Values);
                textureShapeValuesDictionnary.Add(TextureImporterShape.Texture2D | TextureImporterShape.TextureCube, s2D_Cube_Values);

                platformSettingsSelection = new GUIContent[TexturePlatformSettingsModal.validBuildPlatform.Length];
                for (var i = 0; i < TexturePlatformSettingsModal.validBuildPlatform.Length; ++i)
                {
                    platformSettingsSelection[i] = new GUIContent(TexturePlatformSettingsModal.validBuildPlatform[i].buildTargetName);
                }
            }
        }

        static Styles s_Styles;

        static Styles styles
        {
            get
            {
                if (s_Styles == null)
                    s_Styles = new Styles();
                return s_Styles;
            }
        }
        
        class AsepriteImporterEditorFoldOutState
        {
            readonly SavedBool m_GeneralFoldout;
            readonly SavedBool m_LayerImportFoldout;
            readonly SavedBool m_GenerateAssetFoldout;
            readonly SavedBool m_AdvancedFoldout;
            readonly SavedBool m_TextureFoldout;
            readonly SavedBool m_PlatformSettingsFoldout;

            public AsepriteImporterEditorFoldOutState()
            {
                m_GeneralFoldout = new SavedBool("AsepriteImporterEditor.m_GeneralFoldout", true);
                m_LayerImportFoldout = new SavedBool("PSDImporterEditor.m_LayerImportFoldout", true);
                m_GenerateAssetFoldout = new SavedBool("AsepriteImporterEditor.m_ExportAssetFoldout", true);
                m_AdvancedFoldout = new SavedBool("AsepriteImporterEditor.m_AdvancedFoldout", false);
                m_TextureFoldout = new SavedBool("AsepriteImporterEditor.m_TextureFoldout", false);
                m_PlatformSettingsFoldout = new SavedBool("AsepriteImporterEditor.m_PlatformSettingsFoldout", false);
            }
            
            static bool DoFoldout(GUIContent title, bool state)
            {
                InspectorUtils.DrawSplitter();
                return InspectorUtils.DrawHeaderFoldout(title, state);
            }
            
            public bool DoGeneralUI(GUIContent title)
            {
                m_GeneralFoldout.value = DoFoldout(title, m_GeneralFoldout.value);
                return m_GeneralFoldout.value;
            }
            
            public bool DoLayerImportUI(GUIContent title)
            {
                m_LayerImportFoldout.value = DoFoldout(title, m_LayerImportFoldout.value);
                return m_LayerImportFoldout.value;
            }
            
            public bool DoGenerateAssetUI(GUIContent title)
            {
                m_GenerateAssetFoldout.value = DoFoldout(title, m_GenerateAssetFoldout.value);
                return m_GenerateAssetFoldout.value;
            }            

            public bool DoAdvancedUI(GUIContent title)
            {
                m_AdvancedFoldout.value = DoFoldout(title, m_AdvancedFoldout.value);
                return m_AdvancedFoldout.value;
            }

            public bool DoPlatformSettingsUI(GUIContent title)
            {
                m_PlatformSettingsFoldout.value = DoFoldout(title, m_PlatformSettingsFoldout.value);
                return m_PlatformSettingsFoldout.value;
            }
            
            public bool DoTextureUI(GUIContent title)
            {
                m_TextureFoldout.value = DoFoldout(title, m_TextureFoldout.value);
                return m_TextureFoldout.value;
            }

            class SavedBool
            {
                readonly string m_Name;
                bool m_Value;
                bool m_Loaded;

                public SavedBool(string name, bool value)
                {
                    m_Name = name;
                    m_Loaded = false;
                    m_Value = value;
                }

                void Load()
                {
                    if (m_Loaded)
                        return;

                    m_Loaded = true;
                    m_Value = EditorPrefs.GetBool(m_Name, m_Value);
                }

                public bool value
                {
                    get
                    {
                        Load(); 
                        return m_Value;
                    }
                    set
                    {
                        Load();
                        if (m_Value == value)
                            return;
                        m_Value = value;
                        EditorPrefs.SetBool(m_Name, value);
                    }
                }

                public static implicit operator bool(SavedBool s)
                {
                    return s.value;
                }
            }
        }
    }
}