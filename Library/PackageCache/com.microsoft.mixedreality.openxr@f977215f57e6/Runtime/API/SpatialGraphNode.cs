// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.Scripting;

namespace Microsoft.MixedReality.OpenXR
{
    [Preserve]
    public class SpatialGraphNode
    {
        /// <summary>
        /// Creating a SpatialGraphNode with given static node id, or return null upon failure.
        /// </summary>
        static public SpatialGraphNode FromStaticNodeId(System.Guid id)
        {
            if (NativeLib.TryCreateSpaceFromStaticNodeId(token, id, out ulong spaceHandle))
            {
                return new SpatialGraphNode()
                {
                    Id = id,
                    m_spaceHandle = spaceHandle,
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the id of the SpatialGraphNode
        /// </summary>
        public System.Guid Id { get; private set; } = System.Guid.Empty;

        /// <summary>
        /// Locate the SpatialGraphNode at the give time.  Return true if the output pose is valid to use or false indicating the node lost tracking.
        /// </summary>
        public bool TryLocate(FrameTime time, out Pose pose)
        {
            return NativeLib.TryLocateSpace(token, m_spaceHandle, time, out pose);
        }

        private SpatialGraphNode() { }
        private ulong m_spaceHandle = 0;
        private const NativeLibToken token = NativeLibToken.HoloLens;

    }
}
