// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Microsoft.MixedReality.OpenXR
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Hand Tracking plugin",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "Microsoft",
        Desc = "Supports articulated hand tracking with 26 hand joints.",
        DocumentationLink = "https://aka.ms/openxr-unity",
        CustomRuntimeLoaderBuildTargets = null,
        OpenxrExtensionStrings = requestedExtensions,
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.9.3")]
#endif
    [NativeLibToken(NativeLibToken = NativeLibToken.HandTracking)]
    internal class HandTrackingFeaturePlugin : OpenXRFeaturePlugin<HandTrackingFeaturePlugin>
    {
        internal const string featureId = "com.microsoft.openxr.feature.handtracking";
        private const string requestedExtensions = "XR_EXT_hand_tracking XR_MSFT_hand_tracking_mesh";

        private HandTrackingSubsystemController m_handTrackingSubsystemController;

        HandTrackingFeaturePlugin()
        {
            AddSubsystemController(m_handTrackingSubsystemController = new HandTrackingSubsystemController(this));
        }
    }
}
