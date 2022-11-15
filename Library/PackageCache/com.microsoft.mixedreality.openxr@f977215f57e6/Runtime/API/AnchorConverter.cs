// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    [Preserve]
    public static class AnchorConverter
    {
        /// <summary>
        /// Get the OpenXR handle of the given nativePtr from ARAnchor or XRAnchor object if available, or return 0.
        /// </summary>
        public static ulong ToOpenXRHandle(IntPtr nativePtr)
        {
            if (nativePtr == null)
                return 0;

            NativeAnchorData data = Marshal.PtrToStructure<NativeAnchorData>(nativePtr);
            if (data.version == 1)
            {
                return data.anchorHandle;
            }
            return 0;
        }


        /// <summary>
        /// Get a COM wrapper object of Windows.Perception.Spatial.SpatialAnchor from the given ARAnchor's nativePtr.
        /// If failed, the function returns nullptr.
        /// </summary>
        /// <param name="nativePtr">Must be either XRAnchor.nativePtr or ARAnchor.nativePtr.</param>
        public static object ToPerceptionSpatialAnchor(IntPtr nativePtr)
        {
            HoloLensFeaturePlugin feature = OpenXRSettings.Instance.GetFeature<HoloLensFeaturePlugin>();
            if (feature != null && feature.enabled && nativePtr != IntPtr.Zero)
            {
                IntPtr unknown = feature.TryAcquirePerceptionSpatialAnchor(AnchorConverter.ToOpenXRHandle(nativePtr));
                if (unknown != IntPtr.Zero)
                {
                    object result = Marshal.GetObjectForIUnknown(unknown);
                    Marshal.Release(unknown);   // Balance the ref count because "feature.TryAcquire" increment it on return.
                    return result;
                }
            }
            return null;
        }
    }

    namespace ARSubsystems
    {
        [Preserve]
        public static class XRAnchorExtensions
        {
            /// <summary>
            /// Get the native OpenXR handle of the given XRAnchor object if available, or return 0.
            /// </summary>
            public static ulong GetOpenXRHandle(this UnityEngine.XR.ARSubsystems.XRAnchor anchor)
            {
                return anchor == null ? 0 : AnchorConverter.ToOpenXRHandle(anchor.nativePtr);
            }
        }
    }

    namespace ARFoundation
    {
        [Preserve]
        public static class ARAnchorExtensions
        {
            /// <summary>
            /// Get the native OpenXR handle of the given ARAnchor object if available, or return 0.
            /// </summary>
            public static ulong GetOpenXRHandle(this UnityEngine.XR.ARFoundation.ARAnchor anchor)
            {
                return anchor == null ? 0 : AnchorConverter.ToOpenXRHandle(anchor.nativePtr);
            }
        }
    }
}
