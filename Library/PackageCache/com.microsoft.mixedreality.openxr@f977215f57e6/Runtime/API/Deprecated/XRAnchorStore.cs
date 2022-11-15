// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.ARSubsystems
{
    [Preserve]
    public static class AnchorSubsystemExtensions
    {
        [System.Obsolete("The extension function LoadAnchorStoreAsync in namespace Microsoft.MixedReality.ARSubsystems is obsolete and will be removed in future releases. " +
            "Use the version in namespace Microsoft.MixedReality.OpenXR.ARSubsystems instead.")]
        public static Task<XRAnchorStore> LoadAnchorStoreAsync(this XRAnchorSubsystem anchorSubsystem)
        {
            return XRAnchorStore.LoadAsync(anchorSubsystem);
        }
    }

    [System.Obsolete("The type XRAnchorStore in namespace Microsoft.MixedReality.ARSubsystems is obsolete and will be removed in future releases. " +
        "Use the version in namespace Microsoft.MixedReality.OpenXR instead.")]
    public class XRAnchorStore : OpenXR.XRAnchorStore
    {
        internal new static async Task<XRAnchorStore> LoadAsync(XRAnchorSubsystem anchorSubsystem)
        {
            OpenXR.OpenXRAnchorStore openxrAnchorStore = await OpenXR.OpenXRAnchorStoreFactory.LoadAnchorStoreAsync(anchorSubsystem);
            return new XRAnchorStore(openxrAnchorStore);
        }

        private XRAnchorStore(OpenXR.OpenXRAnchorStore openxrAnchorStore)
            : base(openxrAnchorStore)
        {
        }

        [System.Obsolete("The TryPersistAnchor(string, TrackableId) function is obsolete and will be removed in future releases. " +
        "Use the version TryPersistAnchor(TrackableId, string) instead.")]
        public bool TryPersistAnchor(string name, TrackableId trackableId) => TryPersistAnchor(trackableId, name);
    }

} // namespace Microsoft.MixedReality.ARSubsystems
