// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using static UnityEngine.XR.OpenXR.Features.OpenXRFeature;

namespace Microsoft.MixedReality.OpenXR
{
    internal class EditorRemotingValidator
    {
        internal static void GetValidationChecks(OpenXRFeature feature, List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            results.Add(new ValidationRule(feature)
            {
                message = "Using editor remoting to debug HoloLens 2 applications requires you to also enable the " +
                "following HoloLens features in the `Standalone settings` tab, because the Unity editor runs as a standalone application." +
                "\n  - Eye Gaze Interaction Profile," +
                "\n  - Hand Tracking plugin," +
                "\n  - HoloLens OpenXR plugin," +
                "\n  - Microsoft Hand Interaction Profile.",
                error = true,
                checkPredicate = () =>
                {
                    EditorBuildSettings.TryGetConfigObject(UnityEngine.XR.OpenXR.Constants.k_SettingsKey, out Object obj);
                    if (obj is UnityEngine.XR.OpenXR.IPackageSettings packageSettings)
                    {
                        OpenXRSettings openxrSettings = packageSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
                        if (openxrSettings != null &&
                            IsFeatureEnabled<HoloLensFeaturePlugin>(openxrSettings) &&
                            IsFeatureEnabled<HandTrackingFeaturePlugin>(openxrSettings) &&
                            IsFeatureEnabled<EyeGazeInteraction>(openxrSettings) &&
                            IsFeatureEnabled<MicrosoftHandInteraction>(openxrSettings))
                        {
                            return true;
                        }
                    }
                    return false;
                },
                fixIt = null
            });

            results.Add(new ValidationRule(feature)
            {
                message = "Using editor remoting to debug HoloLens 2 applications requires " +
                          "the `Remote Host Name` in `Settings` below to match the IP address displayed in " +
                          "the Holographic Remoting Player running on your HoloLens 2 device.",
                error = true,
                checkPredicate = () =>
                {
                    EditorBuildSettings.TryGetConfigObject(UnityEngine.XR.OpenXR.Constants.k_SettingsKey, out Object obj);
                    if (obj is UnityEngine.XR.OpenXR.IPackageSettings packageSettings)
                    {
                        OpenXRSettings openxrSettings = packageSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
                        Remoting.EditorRemotingPlugin remotingFeature = openxrSettings.GetFeature<Remoting.EditorRemotingPlugin>();
                        if (remotingFeature != null)
                        {
                            return remotingFeature.HasValidSettings();

                        }
                    }
                    return false;
                },
                fixIt = null
            });
        }

        private static bool IsFeatureEnabled<T>(OpenXRSettings openxrSettings) where T : OpenXRFeature
        {
            var feature = openxrSettings.GetFeature<T>();
            return feature != null && feature.enabled;
        }
    }
}
#endif
