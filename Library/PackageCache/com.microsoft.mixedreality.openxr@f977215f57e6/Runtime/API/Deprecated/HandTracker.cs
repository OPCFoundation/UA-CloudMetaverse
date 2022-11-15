// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Preview
{

    /// <summary>
    /// Represents different possible hand poses.
    /// </summary>
    /// <remarks>See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandPoseTypeMSFT for more information.</remarks>
    [Obsolete("The type Preview.HandPoseType is obsolete and will be removed in future releases. Use the non-Preview namespace type instead.")]
    public enum HandPoseType
    {
        /// <summary>
        /// Represents a hand pose provided by actual tracking of the user's hand.
        /// </summary>
        Tracked = 0,

        /// <summary>
        /// Represents a stable reference hand pose in a relaxed open hand shape.
        /// </summary>
        ReferenceOpenPalm,
    }

    [Obsolete("The type Preview.HandTracker is obsolete and will be removed in future releases. Use the non-Preview namespace type instead.")]
    public class HandTracker : OpenXR.HandTracker
    {
        private readonly HandPoseType handPoseType;
        private readonly OpenXR.HandJointLocation[] handJointLocations = new OpenXR.HandJointLocation[JointCount];

        public HandTracker(Handedness handedness, HandPoseType handPoseType = HandPoseType.Tracked) : base((OpenXR.Handedness)handedness)
        {
            this.handPoseType = handPoseType;
        }

        /// <summary>
        /// Fills the passed-in array with current hand joint locations, if possible.
        /// </summary>
        /// <param name="handJointLocations">An array of HandJointLocations, in order according to the HandJoint enum.</param>
        /// <returns>True if hand joints could be located; false otherwise.</returns>
        [Obsolete("The type Preview.HandJointLocation is obsolete and will be removed in future releases. Use the non-Preview namespace instead.")]
        public bool TryLocateHandJoints(OpenXR.FrameTime frameTime, HandJointLocation[] handJointLocations)
        {
            if (handPoseType == HandPoseType.ReferenceOpenPalm)
            {
                throw new NotImplementedException("ReferenceOpenPalm is not yet implemented.");
            }

            if (TryLocateHandJoints(frameTime, this.handJointLocations))
            {
                for (int i = 0; i < JointCount; i++)
                {
                    handJointLocations[i] = new HandJointLocation(this.handJointLocations[i]);
                }
                return true;
            }
            return false;
        }
    }

    [Obsolete("The type Preview.Handedness is obsolete and will be removed in future releases. Use the non-Preview namespace type instead.")]
    public enum Handedness
    {
        Left = 0,
        Right
    }

    /// <summary>
    /// The supported tracked hand joints in OpenXR.
    /// </summary>
    /// <remarks>See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointEXT for more information.</remarks>
    [Obsolete("The type Preview.HandJoint is obsolete and will be removed in future releases. Use the non-Preview namespace type instead.")]
    public enum HandJoint
    {
        /// <summary>
        /// The palm.
        /// </summary>
        Palm,
        /// <summary>
        /// The wrist.
        /// </summary>
        Wrist,
        /// <summary>
        /// The lowest joint of the thumb.
        /// </summary>
        ThumbMetacarpal,
        /// <summary>
        /// The thumb's second (middle-ish) joint.
        /// </summary>
        ThumbProximal,
        /// <summary>
        /// The thumb's first (furthest) joint.
        /// </summary>
        ThumbDistal,
        /// <summary>
        /// The tip of the thumb.
        /// </summary>
        ThumbTip,
        /// <summary>
        /// The lowest joint of the index finger.
        /// </summary>
        IndexMetacarpal,
        /// <summary>
        /// The knuckle joint of the index finger.
        /// </summary>
        IndexProximal,
        /// <summary>
        /// The middle joint of the index finger.
        /// </summary>
        IndexIntermediate,
        /// <summary>
        /// The joint nearest the tip of the index finger.
        /// </summary>
        IndexDistal,
        /// <summary>
        /// The tip of the index finger.
        /// </summary>
        IndexTip,
        /// <summary>
        /// The lowest joint of the middle finger.
        /// </summary>
        MiddleMetacarpal,
        /// <summary>
        /// The proximal joint of the middle finger. 
        /// </summary>
        MiddleProximal,
        /// <summary>
        /// The middle joint of the middle finger.
        /// </summary>
        MiddleIntermediate,
        /// <summary>
        /// The joint nearest the tip of the middle finger.
        /// </summary>
        MiddleDistal,
        /// <summary>
        /// The tip of the middle finger.
        /// </summary>
        MiddleTip,
        /// <summary>
        /// The lowest joint of the ring finger.
        /// </summary>
        RingMetacarpal,
        /// <summary>
        /// The knuckle of the ring finger.
        /// </summary>
        RingProximal,
        /// <summary>
        /// The middle joint of the ring finger.
        /// </summary>
        RingIntermediate,
        /// <summary>
        /// The joint nearest the tip of the ring finger.
        /// </summary>
        RingDistal,
        /// <summary>
        /// The tip of the ring finger.
        /// </summary>
        RingTip,
        /// <summary>
        /// The lowest joint of the little finger.
        /// </summary>
        LittleMetacarpal,
        /// <summary>
        /// The knuckle joint of the little finger.
        /// </summary>
        LittleProximal,
        /// <summary>
        /// The middle joint of the little finger.
        /// </summary>
        LittleIntermediate,
        /// <summary>
        /// The joint nearest the tip of the little finger.
        /// </summary>
        LittleDistal,
        /// <summary>
        /// The tip of the little finger.
        /// </summary>
        LittleTip,
    }

    /// <summary>
    /// Represents locational data for a hand joint.
    /// </summary>
    [Obsolete("The type Preview.HandJointLocation is obsolete and will be removed in future releases. Use the non-Preview namespace type instead.")]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HandJointLocation
    {
        public PoseFlags PoseFlags { get; }
        public Quaternion Rotation { get; }
        public Vector3 Position { get; }
        public float Radius { get; }

        private const PoseFlags AllPoseFlags = PoseFlags.OrientationValid & PoseFlags.PositionValid & PoseFlags.OrientationTracked & PoseFlags.PositionTracked;

        internal HandJointLocation(OpenXR.HandJointLocation handJointLocation)
        {
            PoseFlags = handJointLocation.IsTracked ? AllPoseFlags : 0;
            Rotation = handJointLocation.Pose.rotation;
            Position = handJointLocation.Pose.position;
            Radius = handJointLocation.Radius;
        }
    }

    [Obsolete("The type Preview.PoseFlags is obsolete and will be removed in future releases.")]
    [Flags]
    public enum PoseFlags : ulong
    {
        OrientationValid = 1 << 0,
        PositionValid = 1 << 1,
        OrientationTracked = 1 << 2,
        PositionTracked = 1 << 3,
    }
}
