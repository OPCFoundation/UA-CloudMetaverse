// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    internal static class OpenXRAnchorStoreFactory
    {
        private const string PerceptionAnchorInteropExtension = "XR_MSFT_perception_anchor_interop";

        private static Task<OpenXRAnchorStore> anchorStoreLoadTask = null;
        public static Task<OpenXRAnchorStore> LoadAnchorStoreAsync(XRAnchorSubsystem anchorSubsystem)
        {
            if (!(anchorSubsystem is AnchorSubsystem))
            {
                Debug.LogWarning($"LoadAnchorStoreAsync: subsystem is not of type Microsoft.MixedReality.AnchorSubsystem. type: {anchorSubsystem.GetType()}");
                return Task.FromResult<OpenXRAnchorStore>(null);
            }

            bool isExtensionEnabled = OpenXRRuntime.IsExtensionEnabled(PerceptionAnchorInteropExtension);
            if (!isExtensionEnabled)
            {
                Debug.LogWarning($"LoadAnchorStoreAsync: The anchor store is not supported; missing OpenXR extension {PerceptionAnchorInteropExtension}");
                return Task.FromResult<OpenXRAnchorStore>(null);
            }

            if (anchorStoreLoadTask == null)
            {
                HoloLensFeaturePlugin feature = OpenXRSettings.Instance.GetFeature<HoloLensFeaturePlugin>();
                if (feature != null && feature.enabled)
                {
                    anchorStoreLoadTask = Task.Run(() =>
                    {
                        NativeLib.LoadAnchorStore(OpenXRAnchorStore.nativeLibToken); // Blocking, potentially long call
                        return new OpenXRAnchorStore();
                    });
                }
                else
                {
                    Debug.LogWarning($"LoadAnchorStoreAsync: The anchor store is not supported; {nameof(HoloLensFeaturePlugin)} is not enabled");
                    anchorStoreLoadTask = Task.FromResult<OpenXRAnchorStore>(null);
                }
            }
            return anchorStoreLoadTask;
        }
    }

    internal class OpenXRAnchorStore
    {
        internal static readonly NativeLibToken nativeLibToken = HoloLensFeaturePlugin.nativeLibToken;
        private List<string> m_persistedAnchorNamesCache;
        private bool m_persistedAnchorNamesCacheDirty = true;
        private readonly object m_persistedAnchorNamesCacheLock = new object();

        internal OpenXRAnchorStore()
        {
        }

        public IReadOnlyList<string> PersistedAnchorNames
        {
            get
            {
                lock (m_persistedAnchorNamesCacheLock)
                {
                    if (m_persistedAnchorNamesCacheDirty)
                    {
                        UpdatePersistedAnchorNames();
                        m_persistedAnchorNamesCacheDirty = false;
                    }

                    return m_persistedAnchorNamesCache;
                }
            }
        }

        private void UpdatePersistedAnchorNames()
        {
            lock (m_persistedAnchorNamesCacheLock)
            {
                m_persistedAnchorNamesCache = new List<string>();
                uint numPersisted = NativeLib.GetNumPersistedAnchorNames(nativeLibToken);
                for (uint i = 0; i < numPersisted; i++)
                {
                    // A persisted anchor with a name > 255 chars does not appear
                    // to be supported by the anchor store winrt implementation.
                    StringBuilder stringBuilder = new StringBuilder(255);
                    NativeLib.GetPersistedAnchorName(nativeLibToken, i, stringBuilder, (uint)stringBuilder.Capacity);
                    m_persistedAnchorNamesCache.Add(stringBuilder.ToString());
                }
            }
        }

        public TrackableId LoadAnchor(string name)
        {
            return FeatureUtils.ToTrackableId(NativeLib.LoadPersistedAnchor(nativeLibToken, name));
        }

        public bool TryPersistAnchor(string name, TrackableId trackableId)
        {
            lock (m_persistedAnchorNamesCacheLock)
            {
                m_persistedAnchorNamesCacheDirty = true;
                return NativeLib.TryPersistAnchor(nativeLibToken, name, FeatureUtils.ToGuid(trackableId));
            }
        }

        public void UnpersistAnchor(string name)
        {
            lock (m_persistedAnchorNamesCacheLock)
            {
                m_persistedAnchorNamesCacheDirty = true;
                NativeLib.UnpersistAnchor(nativeLibToken, name);
            }
        }

        public void Clear()
        {
            lock (m_persistedAnchorNamesCacheLock)
            {
                m_persistedAnchorNamesCacheDirty = true;
                NativeLib.ClearPersistedAnchors(nativeLibToken);
            }
        }
    }

} // namespace Microsoft.MixedReality.OpenXR
