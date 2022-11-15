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
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NativeAnchorData
    {
        public uint version;  // == 1
        public ulong anchorHandle; // OpenXR XrSpatialAnchor handle
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NativeAnchor
    {
        public Guid id;
        public Pose pose;
        public TrackingState trackingState;
        public IntPtr nativePtr; // pointer to NativeAnchorData
    }

    [Preserve]
    internal class AnchorSubsystem : XRAnchorSubsystem
    {
        public const string Id = "OpenXR Anchors Subsystem";

        private class OpenXRProvider : Provider
        {
            private readonly NativeLibToken nativeLibToken;

            public OpenXRProvider()
            {
                nativeLibToken = HoloLensFeaturePlugin.nativeLibToken;
            }
            public override void Destroy() { }

#if UNITY_2020_2_OR_NEWER
            public override void Start() { }
            public override void Stop() { }
#endif

            public unsafe override TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                uint numAddedAnchors = 0;
                uint numUpdatedAnchors = 0;
                uint numRemovedAnchors = 0;
                NativeLib.GetNumAnchorChanges(nativeLibToken, FrameTime.OnUpdate, ref numAddedAnchors, ref numUpdatedAnchors, ref numRemovedAnchors);

                using (var addedNativeAnchors = new NativeArray<NativeAnchor>((int)numAddedAnchors, allocator, NativeArrayOptions.UninitializedMemory))
                using (var updatedNativeAnchors = new NativeArray<NativeAnchor>((int)numUpdatedAnchors, allocator, NativeArrayOptions.UninitializedMemory))
                using (var removedNativeAnchors = new NativeArray<Guid>((int)numRemovedAnchors, allocator, NativeArrayOptions.UninitializedMemory))
                {
                    if (numAddedAnchors + numUpdatedAnchors + numRemovedAnchors > 0)
                    {
                        NativeLib.GetAnchorChanges(nativeLibToken,
                            NativeArrayUnsafeUtility.GetUnsafePtr(addedNativeAnchors),
                            NativeArrayUnsafeUtility.GetUnsafePtr(updatedNativeAnchors),
                            NativeArrayUnsafeUtility.GetUnsafePtr(removedNativeAnchors));
                    }

                    // Added Anchors
                    var addedAnchors = Array.Empty<XRAnchor>();
                    if (numAddedAnchors > 0)
                    {
                        addedAnchors = new XRAnchor[numAddedAnchors];
                        for (int i = 0; i < numAddedAnchors; ++i)
                            addedAnchors[i] = ToXRAnchor(addedNativeAnchors[i]);
                    }

                    // Updated Anchors
                    var updatedAnchors = Array.Empty<XRAnchor>();
                    if (numUpdatedAnchors > 0)
                    {
                        updatedAnchors = new XRAnchor[numUpdatedAnchors];
                        for (int i = 0; i < numUpdatedAnchors; ++i)
                            updatedAnchors[i] = ToXRAnchor(updatedNativeAnchors[i]);
                    }

                    // Removed Anchors
                    var removedAnchors = Array.Empty<TrackableId>();
                    if (numRemovedAnchors > 0)
                    {
                        removedAnchors = new TrackableId[numRemovedAnchors];
                        for (int i = 0; i < numRemovedAnchors; ++i)
                            removedAnchors[i] = FeatureUtils.ToTrackableId(removedNativeAnchors[i]);
                    }

                    TrackableChanges<XRAnchor> trackableChanges = TrackableChanges<XRAnchor>.CopyFrom(
                        new NativeArray<XRAnchor>(addedAnchors, allocator),
                        new NativeArray<XRAnchor>(updatedAnchors, allocator),
                        new NativeArray<TrackableId>(removedAnchors, allocator),
                        allocator);
                    return trackableChanges;
                }
            }

            private XRAnchor ToXRAnchor(NativeAnchor nativeAnchor)
            {
                var anchorId = FeatureUtils.ToTrackableId(nativeAnchor.id);
                return new XRAnchor(anchorId, nativeAnchor.pose, nativeAnchor.trackingState, nativeAnchor.nativePtr);
            }

            unsafe public override bool TryAddAnchor(Pose pose, out XRAnchor anchor)
            {
                NativeAnchor nativeAnchor = new NativeAnchor();
                bool succeeded = NativeLib.TryAddAnchor(nativeLibToken, FrameTime.OnUpdate, pose.rotation, pose.position, UnsafeUtility.AddressOf(ref nativeAnchor));
                anchor = ToXRAnchor(nativeAnchor);
                return succeeded;
            }

            public override bool TryAttachAnchor(TrackableId trackableToAffix, Pose pose, out XRAnchor anchor)
            {
                return TryAddAnchor(pose, out anchor);
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                return NativeLib.TryRemoveAnchor(nativeLibToken, FeatureUtils.ToGuid(anchorId));
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            XRAnchorSubsystemDescriptor.Create(new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = Id,
#if UNITY_2020_2_OR_NEWER
                providerType = typeof(AnchorSubsystem.OpenXRProvider),
                subsystemTypeOverride = typeof(AnchorSubsystem),
#else
                subsystemImplementationType = typeof(AnchorSubsystem),
#endif
                supportsTrackableAttachments = false
            });
        }

#if !UNITY_2020_2_OR_NEWER
        // Before 2020.2, the provider is created through this callback.
        // After 2020.2, the provider is created by Unity through the XRAnchorSubsystemDescriptor subsystemTypeOverride
        // After 2020.2, our provider can process lifecycle events through the Provider's Start/Stop function
        protected override Provider CreateProvider()
        {
            return new OpenXRProvider();
        }
#endif
    };

    internal class AnchorSubsystemController : SubsystemController
    {
        private readonly NativeLibToken nativeLibToken;
        private static List<XRAnchorSubsystemDescriptor> s_AnchorDescriptors = new List<XRAnchorSubsystemDescriptor>();

        public AnchorSubsystemController(NativeLibToken token, IOpenXRContext context) : base(context)
        {
            nativeLibToken = token;
        }

        public override void OnSubsystemCreate(ISubsystemPlugin plugin)
        {
            if (Context.IsAnchorExtensionSupported)
            {
                plugin.CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(s_AnchorDescriptors, AnchorSubsystem.Id);
                NativeLib.CreateAnchorProvider(nativeLibToken);
            }
        }

        public override void OnSubsystemStart(ISubsystemPlugin plugin)
        {
            plugin.StartSubsystem<XRAnchorSubsystem>();
        }

        public override void OnSubsystemStop(ISubsystemPlugin plugin)
        {
            plugin.StopSubsystem<XRAnchorSubsystem>();
        }

        public override void OnSubsystemDestroy(ISubsystemPlugin plugin)
        {
            plugin.DestroySubsystem<XRAnchorSubsystem>();
        }
    }
}
