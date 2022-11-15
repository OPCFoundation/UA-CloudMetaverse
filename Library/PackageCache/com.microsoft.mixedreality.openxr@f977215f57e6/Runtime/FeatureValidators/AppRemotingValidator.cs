// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features;
using static UnityEngine.XR.OpenXR.Features.OpenXRFeature;

namespace Microsoft.MixedReality.OpenXR
{
    internal class AppRemotingValidator
    {
        internal static void GetValidationChecks(OpenXRFeature feature, List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            results.Add(new ValidationRule(feature)
            {
                message = "App remoting and initialize XR on startup are both enabled. XR initialization should be delayed to connect to a specific IP address for remoting.",
                error = true,
                checkPredicate = () =>
                {
                    XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);
                    return settings != null && !settings.InitManagerOnStart;
                },
                fixIt = () =>
                {
                    XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);
                    if (settings != null)
                    {
                        settings.InitManagerOnStart = false;
                    }
                }
            });
        }
    }
}
#endif