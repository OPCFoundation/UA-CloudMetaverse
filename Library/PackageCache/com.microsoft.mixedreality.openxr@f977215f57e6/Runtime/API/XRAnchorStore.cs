// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.ARFoundation
{
    [Preserve]
    public static class AnchorManagerExtensions
    {
        public static Task<XRAnchorStore> LoadAnchorStoreAsync(this ARAnchorManager anchorManager)
        {
            return XRAnchorStore.LoadAsync(anchorManager.subsystem);
        }
    }
}

namespace Microsoft.MixedReality.OpenXR.ARSubsystems
{
    [Preserve]
    public static class AnchorSubsystemExtensions
    {
        public static Task<XRAnchorStore> LoadAnchorStoreAsync(this XRAnchorSubsystem anchorSubsystem)
        {
            return XRAnchorStore.LoadAsync(anchorSubsystem);
        }
    }
}

namespace Microsoft.MixedReality.OpenXR
{
    [Preserve]
    public class XRAnchorStore
    {
        public IReadOnlyList<string> PersistedAnchorNames => m_openxrAnchorStore.PersistedAnchorNames;

        public TrackableId LoadAnchor(string name) => m_openxrAnchorStore.LoadAnchor(name);
        public bool TryPersistAnchor(TrackableId trackableId, string name) => m_openxrAnchorStore.TryPersistAnchor(name, trackableId);
        public void UnpersistAnchor(string name) => m_openxrAnchorStore.UnpersistAnchor(name);
        public void Clear() => m_openxrAnchorStore.Clear();

        public static async Task<XRAnchorStore> LoadAsync(XRAnchorSubsystem anchorSubsystem)
        {
            OpenXRAnchorStore openxrAnchorStore = await OpenXRAnchorStoreFactory.LoadAnchorStoreAsync(anchorSubsystem);
            return openxrAnchorStore == null ? null : new XRAnchorStore(openxrAnchorStore);
        }

        internal XRAnchorStore(OpenXRAnchorStore openxrAnchorStore)
        {
            m_openxrAnchorStore = openxrAnchorStore;
        }

        private readonly OpenXRAnchorStore m_openxrAnchorStore;
    }
}
