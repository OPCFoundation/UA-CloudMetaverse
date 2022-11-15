// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine.XR.OpenXR.Features;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Microsoft.MixedReality.OpenXR
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "HoloLens OpenXR plugin",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "Microsoft",
        Desc = "HoloLens OpenXR plugin",
        DocumentationLink = "https://aka.ms/openxr-unity",
        CustomRuntimeLoaderBuildTargets = null,
        OpenxrExtensionStrings = requestedExtensions,
        Required = true,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.9.3")]
#endif
    [NativeLibToken(NativeLibToken = NativeLibToken.HoloLens)]
    internal class HoloLensFeaturePlugin : OpenXRFeaturePlugin<HoloLensFeaturePlugin>
    {
        internal const string featureId = "com.microsoft.openxr.feature.hololens";
        private const string requestedExtensions = " XR_MSFT_holographic_window_attachment"
        + " XR_MSFT_unbounded_reference_space"
        + " XR_MSFT_spatial_anchor"
        + " XR_MSFT_secondary_view_configuration"
        + " XR_MSFT_first_person_observer"
        + " XR_MSFT_spatial_graph_bridge"
        + " XR_MSFT_perception_anchor_interop"
        + " XR_MSFT_scene_understanding_preview3";

        private PlaneSubsystemController m_planeSubsystemController;
        private AnchorSubsystemController m_anchorSubsystemController;
        private RaycastSubsystemController m_raycastSubsystemController;
        private MeshSubsystemController m_meshSubsystemController;

        HoloLensFeaturePlugin()
        {
            AddSubsystemController(m_anchorSubsystemController = new AnchorSubsystemController(nativeLibToken, this));
            AddSubsystemController(m_planeSubsystemController = new PlaneSubsystemController(nativeLibToken, this));
            AddSubsystemController(m_raycastSubsystemController = new RaycastSubsystemController(nativeLibToken, this));
            AddSubsystemController(m_meshSubsystemController = new MeshSubsystemController(nativeLibToken, this));
        }

        internal IntPtr TryAcquireSceneCoordinateSystem(Pose poseInScene) {
            return NativeLib.TryAcquireSceneCoordinateSystem(nativeLibToken, poseInScene);
        }

        internal IntPtr TryAcquirePerceptionSpatialAnchor(ulong anchorHandle){
            return NativeLib.TryAcquirePerceptionSpatialAnchor(nativeLibToken, anchorHandle);
        }

#if UNITY_EDITOR
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            HoloLensFeatureValidator.GetValidationChecks(this, results, targetGroup);
        }
#endif
    }
}
