// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    [OpenXRFeatureSet(
        FeatureIds = new string[] {
            // This list is empty because Unity requires any feature in featureset to be "on" by default
            // When Unity supports optional features, we can add app remoting, hand tracking for VR here.
            },
        UiName = "Windows Mixed Reality",
        // This will appear as a tooltip for the (?) icon in the loader UI.
        Description = "Enable the full suite of features for Windows Mixed Reality headsets.",
        // If this is changed, please report the new ID back to Unity so that we can update our list.
        FeatureSetId = featureSetId,
        SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.Standalone }
    )]
    sealed class WMRFeatureSet
    {
        internal const string featureSetId = "com.microsoft.openxr.featureset.wmr";
    }
}
