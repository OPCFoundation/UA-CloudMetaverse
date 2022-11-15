// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    /// <summary>
    /// Provides a menu item for configuring settings according to specified OpenXR devices.
    /// </summary>
    internal static class UpdateSettings
    {
        [MenuItem("Mixed Reality/OpenXR/Apply recommended scene settings for HoloLens 2", false, 11)]
        private static void ApplyHoloLens2CameraSettings()
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.clear;
            ApplyGeneralCameraSettings();
        }

        private static void ApplyGeneralCameraSettings()
        {
            if (!Camera.main.gameObject.GetComponent<TrackedPoseDriver>()
#if USE_ARFOUNDATION
                && !Camera.main.gameObject.GetComponent<UnityEngine.XR.ARFoundation.ARPoseDriver>()
#endif
                )
            {
                Camera.main.gameObject.AddComponent<TrackedPoseDriver>();
            }
        }

        [MenuItem("Mixed Reality/OpenXR/Apply recommended project settings for HoloLens 2", false, 0)]
        private static void ApplyOpenXRSettings()
        {
            XRGeneralSettings standaloneGeneralSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            XRGeneralSettings wsaGeneralSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.WSA);

            if (standaloneGeneralSettings)
            {
                XRPackageMetadataStore.AssignLoader(standaloneGeneralSettings.AssignedSettings, nameof(OpenXRLoader), BuildTargetGroup.Standalone);
                EnableFeatureSet(BuildTargetGroup.Standalone);
            }

            if (wsaGeneralSettings)
            {
                XRPackageMetadataStore.AssignLoader(wsaGeneralSettings.AssignedSettings, nameof(OpenXRLoaderNoPreInit), BuildTargetGroup.WSA);
                EnableFeatureSet(BuildTargetGroup.WSA);
            }

            EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out Object obj);
            if (obj is IPackageSettings packageSettings)
            {
                EnableFeaturesInSettings(packageSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone), OpenXRSettings.DepthSubmissionMode.Depth24Bit);
                EnableFeaturesInSettings(packageSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.WSA), OpenXRSettings.DepthSubmissionMode.Depth16Bit);
            }

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
            QualitySettings.SetQualityLevel(2, true);
            QualitySettings.shadows = ShadowQuality.Disable;    // disable shadow of medium quality.
            QualitySettings.SetQualityLevel(0, true);

            AssetDatabase.SaveAssets();

            PlayerSettings.colorSpace = ColorSpace.Linear;

            SetRealtimeGlobalIllumination(false);
        }

        private static void EnableFeatureSet(BuildTargetGroup target)
        {
            foreach (var featureSet in OpenXRFeatureSetManager.FeatureSetsForBuildTarget(target))
            {
                if (featureSet.featureSetId == HoloLensFeatureSet.featureSetId ||
                    featureSet.featureSetId == WMRFeatureSet.featureSetId)
                {
                    featureSet.isEnabled = true;
                }
                else
                {
                    featureSet.isEnabled = false;
                }
            }
        }

        private static void EnableFeaturesInSettings(OpenXRSettings settings, OpenXRSettings.DepthSubmissionMode depthSubmissionMode)
        {
            if (settings != null)
            {
                FieldInfo renderModeField = typeof(OpenXRSettings).GetField("m_renderMode", BindingFlags.NonPublic | BindingFlags.Instance);
                if (renderModeField != null)
                {
                    renderModeField.SetValue(settings, OpenXRSettings.RenderMode.SinglePassInstanced);
                }

                FieldInfo depthSubmissionModeField = typeof(OpenXRSettings).GetField("m_depthSubmissionMode", BindingFlags.NonPublic | BindingFlags.Instance);
                if (depthSubmissionModeField != null)
                {
                    depthSubmissionModeField.SetValue(settings, depthSubmissionMode);
                }

                foreach (OpenXRFeature feature in settings.GetFeatures())
                {
                    if (feature is HoloLensFeaturePlugin ||
                        feature is HandTrackingFeaturePlugin ||
                        feature is OpenXRInteractionFeature)
                    {
                        feature.enabled = true;
                    }
                }
                EditorUtility.SetDirty(settings);
            }
        }

        /// <summary>
        /// Loads the lightmap settings as a SerializedObject and updates the realtime global illumination setting.
        /// </summary>
        /// <param name="enabled">Whether to enable or disable the realtime global illumination setting.</param>
        private static void SetRealtimeGlobalIllumination(bool enabled)
        {
            MethodInfo getLightmapSettingsMethod = typeof(LightmapEditorSettings).GetMethod("GetLightmapSettings", BindingFlags.Static | BindingFlags.NonPublic);
            if (getLightmapSettingsMethod != null)
            {
                SerializedObject lightmapSettings = new SerializedObject(getLightmapSettingsMethod.Invoke(null, null) as Object);
                SerializedProperty lightmaps = lightmapSettings?.FindProperty("m_GISettings.m_EnableRealtimeLightmaps");
                if (lightmaps != null)
                {
                    lightmaps.boolValue = enabled;
                    lightmapSettings.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogError("Could not find m_GISettings.m_EnableRealtimeLightmaps via reflection. Has this property been removed or renamed?");
                }
            }
            else
            {
                Debug.LogError("Could not find GetLightmapSettings via reflection. Has this method been removed or renamed?");
            }
        }
    }
}
