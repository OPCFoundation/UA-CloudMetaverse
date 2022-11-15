// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    [OpenXRFeatureSet(
        FeatureIds = new string[] {
            HoloLensFeaturePlugin.featureId,
            HandTrackingFeaturePlugin.featureId,
            EyeGazeInteraction.featureId,
            MicrosoftHandInteraction.featureId,
            },
        UiName = "Microsoft HoloLens",
        // This will appear as a tooltip for the (?) icon in the loader UI.
        Description = "Enable the full suite of features for Microsoft HoloLens 2.",
        // If this is changed, please report the new ID back to Unity so that we can update our list.
        FeatureSetId = featureSetId,
        SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.WSA }
    )]
    sealed class HoloLensFeatureSet
    {
        internal const string featureSetId = "com.microsoft.openxr.featureset.hololens";
    }
}
