// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.OpenXR;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Microsoft.MixedReality.OpenXR.Remoting
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Holographic App Remoting",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "Microsoft",
        Desc = "Holographic app remoting plugin for compiled applications",
        DocumentationLink = "https://aka.ms/openxr-unity-app-remoting",
        OpenxrExtensionStrings = requestedExtensions,
        Category = FeatureCategory.Feature,
        Required = false,
        Priority = -100,    // hookup before other plugins so it affects json before GetProcAddr.
        FeatureId = featureId,
        Version = "0.9.3")]
#endif
    [NativeLibToken(NativeLibToken = NativeLibToken.Remoting)]
    internal class AppRemotingPlugin : OpenXRFeaturePlugin<AppRemotingPlugin>
    {
        internal const string featureId = "com.microsoft.openxr.feature.appremoting";
        private const string requestedExtensions = "XR_MSFT_holographic_remoting";

        private bool m_remotingExtensionEnabled = false;
        private bool m_runtimeOverrideAttempted = false;

        private readonly bool m_appRemotingEnabled =
#if UNITY_EDITOR
            false;
#else
            true;
#endif
        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            if (m_appRemotingEnabled && !m_runtimeOverrideAttempted)
            {
                m_runtimeOverrideAttempted = true;
                if (!NativeLib.TryEnableRemotingOverride(nativeLibToken))
                {
                    Debug.LogError($"Failed to enable remoting runtime.");
                }
            }
            return base.HookGetInstanceProcAddr(func);
        }

        protected override bool OnInstanceCreate(ulong instance)
        {
            m_remotingExtensionEnabled = OpenXRRuntime.IsExtensionEnabled("XR_MSFT_holographic_remoting");
            return base.OnInstanceCreate(instance);
        }

        protected override void OnInstanceDestroy(ulong instance)
        {
            m_remotingExtensionEnabled = false;
            if (m_appRemotingEnabled && m_runtimeOverrideAttempted)
            {
                m_runtimeOverrideAttempted = false;
                NativeLib.ResetRemotingOverride(nativeLibToken);
            }
            base.OnInstanceDestroy(instance);
        }

        protected override void OnSystemChange(ulong systemId)
        {
            base.OnSystemChange(systemId);

            if (m_appRemotingEnabled && m_remotingExtensionEnabled)
            {
                NativeLib.ConnectRemoting(nativeLibToken, AppRemoting.Configuration);
            }
        }

        protected override void OnSessionStateChange(int oldState, int newState)
        {
            if (m_appRemotingEnabled && (XrSessionState)newState == XrSessionState.LossPending)
            {
                Debug.LogError($"Cannot establish a connection to Holographic Remoting Player on the target with IP Address {AppRemoting.Configuration.RemoteHostName}:{AppRemoting.Configuration.RemotePort}.");
            }
        }

#if UNITY_EDITOR
        protected override void GetValidationChecks(System.Collections.Generic.List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            AppRemotingValidator.GetValidationChecks(this, results, targetGroup);
        }
#endif
    }
}
