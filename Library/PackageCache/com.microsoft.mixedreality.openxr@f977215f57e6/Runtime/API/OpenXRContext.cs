// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Retrieve the current OpenXR instance, session handles and states
    /// </summary>
    public class OpenXRContext
    {
        /// <summary>
        /// Get the current OpenXR context
        /// </summary>
        public static OpenXRContext Current => m_current;

        /// <summary>
        /// The XrInstance handle, or 0 when instance is not initalized
        /// </summary>
        public ulong Instance => m_feature != null && m_feature.enabled ? m_feature.Instance : 0;

        /// <summary>
        /// The XrSystemId, or 0 when system is not available
        /// </summary>
        public ulong SystemId => m_feature != null && m_feature.enabled ? m_feature.SystemId : 0;

        /// <summary>
        /// The XrSession handle, or 0 when session is not created
        /// </summary>
        public ulong Session => m_feature != null && m_feature.enabled ? m_feature.Session : 0;

        /// <summary>
        /// An XrSpace handle to the reference space of the current Unity scene origin, or 0 when not available.
        /// </summary>
        public ulong SceneOriginSpace => m_feature != null && m_feature.enabled ? m_feature.SceneOriginSpace : 0;

        /// <summary>
        /// Get a function pointer of given function name.  
        /// May return 0 if the function name is not found, or corresponding extension is not enabled.
        /// </summary>
        public IntPtr GetInstanceProcAddr(string functionName) => m_feature != null && m_feature.enabled ? m_feature.GetInstanceProcAddr(functionName) : IntPtr.Zero;

        private static OpenXRContext m_current = new OpenXRContext();
        private readonly HoloLensFeaturePlugin m_feature = OpenXRSettings.Instance.GetFeature<HoloLensFeaturePlugin>();
    }
}
