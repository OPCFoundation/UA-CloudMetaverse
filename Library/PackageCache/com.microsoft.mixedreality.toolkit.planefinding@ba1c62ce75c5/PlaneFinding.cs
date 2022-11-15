// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAwareness.Processing
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OrientedBoundingBox
    {
        public Vector3 Center;
        public Vector3 Extents;
        public Quaternion Rotation;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct BoundedPlane
    {
        public Plane Plane;
        public OrientedBoundingBox Bounds;
        public float Area;
    };

    public class PlaneFinding
    {
        #region Public APIs

        /// <summary>
        /// PlaneFinding is an expensive task that should not be run from Unity's main thread as it
        /// will stall the thread and cause a frame rate dip. Instead, the PlaneFinding APIs should be
        /// exclusively called from background threads. Unfortunately, Unity's built-in data types
        /// (such as MeshFilter) are not thread safe and cannot be accessed from background threads.
        /// The MeshData struct exists to work-around this limitation. When you want to find planes
        /// in a collection of MeshFilter objects, start by constructing a list of MeshData structs
        /// from those MeshFilters. You can then take the resulting list of MeshData structs, and
        /// safely pass it to the FindPlanes() API from a background thread.
        /// </summary>
        public readonly struct MeshData
        {
            internal Matrix4x4 Transform { get; }
            internal Vector3[] Vertices { get; }
            internal Vector3[] Normals { get; }
            internal int[] Indices { get; }

            public MeshData(MeshFilter meshFilter)
            {
                Transform = meshFilter.transform.localToWorldMatrix;
                Vertices = meshFilter.sharedMesh.vertices;
                Normals = meshFilter.sharedMesh.normals;
                Indices = meshFilter.sharedMesh.triangles;
            }

            internal MeshData(Matrix4x4 transform, Vector3[] vertices, Vector3[] normals, int[] indices)
            {
                Transform = transform;
                Vertices = vertices;
                Normals = normals;
                Indices = indices;
            }
        }

        /// <summary>
        /// Finds small planar patches that are contained within individual meshes.  The output of this
        /// API can then be passed to MergeSubPlanes() in order to find larger planar surfaces that
        /// potentially span across multiple meshes.
        /// </summary>
        /// <param name="meshes">
        /// List of meshes to run the plane finding algorithm on.
        /// </param>
        /// <param name="snapToGravityThreshold">
        /// Planes whose normal vectors are within this threshold (in degrees) from vertical/horizontal
        /// will be snapped to be perfectly gravity aligned.  When set to something other than zero, the
        /// bounding boxes for each plane will be gravity aligned as well, rather than rotated for an
        /// optimally tight fit. Pass 0.0 for this parameter to completely disable the gravity alignment
        /// logic.
        /// </param>
        public static BoundedPlane[] FindSubPlanes(List<MeshData> meshes, float snapToGravityThreshold = 0.0f)
        {
            StartPlaneFinding();

            try
            {
                IntPtr pinnedMeshData = PinMeshDataForMarshalling(meshes);
                DLLImports.FindSubPlanes(meshes.Count, pinnedMeshData, snapToGravityThreshold, out int planeCount, out IntPtr planesPtr);
                return MarshalBoundedPlanesFromIntPtr(planesPtr, planeCount);
            }
            finally
            {
                FinishPlaneFinding();
            }
        }

        /// <summary>
        /// Takes the sub-planes returned by one or more previous calls to FindSubPlanes() and merges
        /// them together into larger planes that can potentially span across multiple meshes.
        /// Overlapping sub-planes that have similar plane equations will be merged together to form
        /// larger planes.
        /// </summary>
        /// <param name="subPlanes">
        /// The output from one or more previous calls to FindSubPlanes().
        /// </param>
        /// <param name="snapToGravityThreshold">
        /// Planes whose normal vectors are within this threshold (in degrees) from vertical/horizontal
        /// will be snapped to be perfectly gravity aligned.  When set to something other than zero, the
        /// bounding boxes for each plane will be gravity aligned as well, rather than rotated for an
        /// optimally tight fit. Pass 0.0 for this parameter to completely disable the gravity alignment
        /// logic.
        /// </param>
        /// <param name="minArea">
        /// While merging sub-planes together, any candidate merged plane whose constituent mesh
        /// triangles have a total area less than this threshold are ignored.
        /// </param>
        public static BoundedPlane[] MergeSubPlanes(BoundedPlane[] subPlanes, float snapToGravityThreshold = 0.0f, float minArea = 0.0f)
        {
            StartPlaneFinding();

            try
            {
                DLLImports.MergeSubPlanes(subPlanes.Length, PinObject(subPlanes), minArea, snapToGravityThreshold, out int planeCount, out IntPtr planesPtr);
                return MarshalBoundedPlanesFromIntPtr(planesPtr, planeCount);
            }
            finally
            {
                FinishPlaneFinding();
            }
        }

        /// <summary>
        /// Convenience wrapper that executes FindSubPlanes followed by MergeSubPlanes via a single
        /// call into native code (which improves performance by avoiding a bunch of unnecessary data
        /// marshalling and a managed-to-native transition).
        /// </summary>
        /// <param name="meshes">
        /// List of meshes to run the plane finding algorithm on.
        /// </param>
        /// <param name="snapToGravityThreshold">
        /// Planes whose normal vectors are within this threshold (in degrees) from vertical/horizontal
        /// will be snapped to be perfectly gravity aligned.  When set to something other than zero, the
        /// bounding boxes for each plane will be gravity aligned as well, rather than rotated for an
        /// optimally tight fit. Pass 0.0 for this parameter to completely disable the gravity alignment
        /// logic.
        /// </param>
        /// <param name="minArea">
        /// While merging sub-planes together, any candidate merged plane whose constituent mesh
        /// triangles have a total area less than this threshold are ignored.
        /// </param>
        public static BoundedPlane[] FindPlanes(List<MeshData> meshes, float snapToGravityThreshold = 0.0f, float minArea = 0.0f)
        {
            StartPlaneFinding();

            try
            {
                IntPtr pinnedMeshData = PinMeshDataForMarshalling(meshes);
                DLLImports.FindPlanes(meshes.Count, pinnedMeshData, minArea, snapToGravityThreshold, out int planeCount, out IntPtr planesPtr);
                return MarshalBoundedPlanesFromIntPtr(planesPtr, planeCount);
            }
            finally
            {
                FinishPlaneFinding();
            }
        }

        #endregion

        #region Internal

        private static readonly object FindPlanesLock = new object();
        private static readonly List<GCHandle> ReusedPinnedMemoryHandles = new List<GCHandle>();

        private static bool findPlanesRunning = false;
        private static DLLImports.ImportedMeshData[] reusedImportedMeshesForMarshalling = null;

        /// <summary>
        /// Validate that no other PlaneFinding API call is currently in progress. As a performance
        /// optimization to avoid unnecessarily thrashing the garbage collector, each call into the
        /// PlaneFinding DLL reuses a couple of static data structures. As a result, we can't handle
        /// multiple concurrent calls into these APIs.
        /// </summary>
        private static void StartPlaneFinding()
        {
            lock (FindPlanesLock)
            {
                if (findPlanesRunning)
                {
                    throw new Exception("PlaneFinding is already running. You can not call these APIs from multiple threads.");
                }
                findPlanesRunning = true;
            }
        }

        /// <summary>
        /// Cleanup after finishing a PlaneFinding API call by unpinning any memory that was pinned
        /// for the call into the driver, and then reset the findPlanesRunning bool.
        /// </summary>
        private static void FinishPlaneFinding()
        {
            UnpinAllObjects();
            findPlanesRunning = false;
        }

        /// <summary>
        /// Pins the specified object so that the backing memory can not be relocated, adds the pinned
        /// memory handle to the tracking list, and then returns that address of the pinned memory so
        /// that it can be passed into the DLL to be access directly from native code.
        /// </summary>
        private static IntPtr PinObject(object obj)
        {
            GCHandle h = GCHandle.Alloc(obj, GCHandleType.Pinned);
            ReusedPinnedMemoryHandles.Add(h);
            return h.AddrOfPinnedObject();
        }

        /// <summary>
        /// Unpins all of the memory previously pinned by calls to PinObject().
        /// </summary>
        private static void UnpinAllObjects()
        {
            for (int i = 0; i < ReusedPinnedMemoryHandles.Count; ++i)
            {
                ReusedPinnedMemoryHandles[i].Free();
            }
            ReusedPinnedMemoryHandles.Clear();
        }

        /// <summary>
        /// Copies the supplied mesh data into the reusedMeshesForMarshalling array. All managed arrays
        /// are pinned so that the marshalling only needs to pass a pointer and the native code can
        /// reference the memory in place without needing the marshaller to create a complete copy of
        /// the data.
        /// </summary>
        private static IntPtr PinMeshDataForMarshalling(List<MeshData> meshes)
        {
            // if we have a big enough array reuse it, otherwise create new
            if (reusedImportedMeshesForMarshalling == null || reusedImportedMeshesForMarshalling.Length < meshes.Count)
            {
                reusedImportedMeshesForMarshalling = new DLLImports.ImportedMeshData[meshes.Count];
            }

            for (int i = 0; i < meshes.Count; ++i)
            {
                reusedImportedMeshesForMarshalling[i] = new DLLImports.ImportedMeshData()
                {
                    transform = meshes[i].Transform,
                    vertCount = meshes[i].Vertices.Length,
                    indexCount = meshes[i].Indices.Length,
                    verts = PinObject(meshes[i].Vertices),
                    normals = PinObject(meshes[i].Normals),
                    indices = PinObject(meshes[i].Indices),
                };
            }

            return PinObject(reusedImportedMeshesForMarshalling);
        }

        /// <summary>
        /// Marshals BoundedPlane data returned from a DLL API call into a managed BoundedPlane array
        /// and then frees the memory that was allocated within the DLL.
        /// </summary>
        private static BoundedPlane[] MarshalBoundedPlanesFromIntPtr(IntPtr outArray, int size)
        {
            BoundedPlane[] resArray = new BoundedPlane[size];
            int structsize = Marshal.SizeOf(typeof(BoundedPlane));
            IntPtr current = outArray;
            for (int i = 0; i < size; i++)
            {
                resArray[i] = (BoundedPlane)Marshal.PtrToStructure(current, typeof(BoundedPlane));
                current = (IntPtr)((long)current + structsize);
            }
            Marshal.FreeCoTaskMem(outArray);
            return resArray;
        }

        /// <summary>
        /// Raw PlaneFinding.dll imports
        /// </summary>
        private class DLLImports
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct ImportedMeshData
            {
                public Matrix4x4 transform;
                public Int32 vertCount;
                public Int32 indexCount;
                public IntPtr verts;
                public IntPtr normals;
                public IntPtr indices;
            };

            [DllImport("PlaneFinding")]
            public static extern void FindPlanes(
                [In] int meshCount,
                [In] IntPtr meshes,
                [In] float minArea,
                [In] float snapToGravityThreshold,
                [Out] out int planeCount,
                [Out] out IntPtr planesPtr);

            [DllImport("PlaneFinding")]
            public static extern void FindSubPlanes(
                [In] int meshCount,
                [In] IntPtr meshes,
                [In] float snapToGravityThreshold,
                [Out] out int planeCount,
                [Out] out IntPtr planesPtr);

            [DllImport("PlaneFinding")]
            public static extern void MergeSubPlanes(
                [In] int subPlaneCount,
                [In] IntPtr subPlanes,
                [In] float minArea,
                [In] float snapToGravityThreshold,
                [Out] out int planeCount,
                [Out] out IntPtr planesPtr);
        }

        #endregion
    }
}