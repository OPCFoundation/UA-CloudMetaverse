// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR
{
    [Preserve]
    internal class RaycastSubsystem : XRRaycastSubsystem
    {
        public const string Id = "OpenXR Raycasting";

        private class OpenXRProvider : Provider
        {
            public override bool TryAddRaycast(Vector2 screenPoint, float estimatedDistance, out XRRaycast raycast)
            {
                Debug.LogError("Persistent raycasts are not supported; use single raycasts instead. More information about single and persistent raycasts is available at https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/raycast-manager.html#single-raycasts.");
                raycast = default;
                return false;
            }
            public override bool TryAddRaycast(Ray ray, float estimatedDistance, out XRRaycast raycast)
            {
                Debug.LogError("Persistent raycasts are not supported; use single raycasts instead. More information about single and persistent raycasts is available at https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/raycast-manager.html#single-raycasts.");
                raycast = default;
                return false;
            }
            public override void RemoveRaycast(TrackableId trackableId)
            {
                Debug.LogError("Persistent raycasts are not supported; use single raycasts instead. More information about single and persistent raycasts is available at https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/raycast-manager.html#single-raycasts.");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = Id,
#if UNITY_2020_2_OR_NEWER
                    providerType = typeof(RaycastSubsystem.OpenXRProvider),
                    subsystemTypeOverride = typeof(RaycastSubsystem),
#else
                subsystemImplementationType = typeof(RaycastSubsystem),
#endif

                // Setting these supported flags to false triggers a fallback in ARRaycastManager,
                // using ARPlaneManager::Raycast for computing raycasts.
                supportedTrackableTypes = TrackableType.Planes,
                supportsTrackedRaycasts = false,
                supportsViewportBasedRaycast = false,
                supportsWorldBasedRaycast = false
            });
        }

#if !UNITY_2020_2_OR_NEWER
        // Before 2020.2, the provider is created through this callback.
        // After 2020.2, the provider is created by Unity through the XRRaycastSubsystemDescriptor subsystemTypeOverride
        // After 2020.2, our provider can process lifecycle events through the Provider's Start/Stop function
        protected override Provider CreateProvider()
        {
            return new OpenXRProvider();
        }
#endif
    };

    internal class RaycastSubsystemController : SubsystemController
    {
        private readonly NativeLibToken nativeLibToken;
        private static List<XRRaycastSubsystemDescriptor> s_RaycastDescriptors = new List<XRRaycastSubsystemDescriptor>();

        public RaycastSubsystemController(NativeLibToken token, IOpenXRContext context) : base(context)
        {
            nativeLibToken = token;
        }

        public override void OnSubsystemCreate(ISubsystemPlugin plugin)
        {
            plugin.CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(s_RaycastDescriptors, RaycastSubsystem.Id);
        }

        public override void OnSubsystemDestroy(ISubsystemPlugin plugin)
        {
            plugin.DestroySubsystem<XRRaycastSubsystem>();
        }
    }
}
