// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Microsoft.MixedReality.OpenXR
{
    internal abstract class OpenXRFeaturePlugin<TPlugin>
        : OpenXRFeature, IOpenXRContext, ISubsystemPlugin where TPlugin : OpenXRFeaturePlugin<TPlugin>
    {
        internal static readonly NativeLibToken nativeLibToken;

        private List<SubsystemController> m_subsystemControllers = new List<SubsystemController>();

        public ulong Instance { get; private set; }
        public ulong SystemId { get; private set; }
        public ulong Session { get; private set; }
        public XrSessionState SessionState { get; private set; }
        public ulong SceneOriginSpace { get; private set; }

        public bool IsAnchorExtensionSupported { get; private set; }

        public IntPtr GetInstanceProcAddr(string functionName)
        {
            return Instance == 0
                ? IntPtr.Zero
                : NativeLib.GetInstanceProcAddr(Instance, OpenXRFeature.xrGetInstanceProcAddr, functionName);
        }

        static OpenXRFeaturePlugin()
        {
            NativeLibTokenAttribute attribute = typeof(TPlugin).GetCustomAttributes(
                typeof(NativeLibTokenAttribute), inherit: false).FirstOrDefault() as NativeLibTokenAttribute;
            if (attribute == null)
            {
                Debug.LogError($"{typeof(TPlugin).Name} lacks NativeLibToken attribute");
                return;
            }
            nativeLibToken = attribute.NativeLibToken;
        }

        protected OpenXRFeaturePlugin()
        {
            if (enabled)
            {
                NativeLib.InitializeNativeLibToken(nativeLibToken);
            }
        }

        protected void AddSubsystemController(SubsystemController subsystemController)
        {
            m_subsystemControllers.Add(subsystemController);
        }

        private bool IsExtensionEnabled(string extensionName, uint minimumRevision = 1)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(extensionName))
                return false;

            return OpenXRRuntime.GetExtensionVersion(extensionName) >= minimumRevision;
        }

        protected override void OnSubsystemCreate()
        {
            m_subsystemControllers.ForEach(controller => controller.OnSubsystemCreate(this));
        }

        protected override void OnSubsystemStart()
        {
            m_subsystemControllers.ForEach(controller => controller.OnSubsystemStart(this));
        }

        protected override void OnSubsystemStop()
        {
            m_subsystemControllers.ForEach(controller => controller.OnSubsystemStop(this));
        }

        protected override void OnSubsystemDestroy()
        {
            m_subsystemControllers.ForEach(controller => controller.OnSubsystemDestroy(this));
        }

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            return NativeLib.HookGetInstanceProcAddr(nativeLibToken, func);
        }

        protected override bool OnInstanceCreate(ulong instance)
        {
            Instance = instance;
            NativeLib.SetXrInstance(nativeLibToken, instance);

            IsAnchorExtensionSupported = IsExtensionEnabled("XR_MSFT_spatial_anchor");
            return true;
        }

        protected override void OnInstanceDestroy(ulong instance)
        {
            SystemId = 0;
            NativeLib.SetXrSystemId(nativeLibToken, 0);

            Instance = 0;
            NativeLib.SetXrInstance(nativeLibToken, 0);
        }

        protected override void OnSystemChange(ulong systemId)
        {
            SystemId = systemId;
            NativeLib.SetXrSystemId(nativeLibToken, systemId);
        }

        protected override void OnSessionCreate(ulong session)
        {
            Session = session;
            NativeLib.SetXrSession(nativeLibToken, session);
        }

        protected override void OnSessionBegin(ulong session)
        {
            NativeLib.SetXrSessionRunning(nativeLibToken, true);
        }

        protected override void OnSessionStateChange(int oldState, int newState)
        {
            SessionState = (XrSessionState)newState;
            NativeLib.SetSessionState(nativeLibToken, (uint)newState);
        }

        protected override void OnSessionEnd(ulong session)
        {
            NativeLib.SetXrSessionRunning(nativeLibToken, false);
        }

        protected override void OnSessionDestroy(ulong session)
        {
            Session = 0;
            NativeLib.SetXrSession(nativeLibToken, 0);
        }

        protected override void OnAppSpaceChange(ulong sceneOriginSpace)
        {
            SceneOriginSpace = sceneOriginSpace;
            NativeLib.SetSceneOriginSpace(nativeLibToken, sceneOriginSpace);
        }

        void ISubsystemPlugin.CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id) =>
            base.CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);

        void ISubsystemPlugin.StartSubsystem<T>() => base.StartSubsystem<T>();

        void ISubsystemPlugin.StopSubsystem<T>() => base.StopSubsystem<T>();

        void ISubsystemPlugin.DestroySubsystem<T>() => base.DestroySubsystem<T>();
    }
}