# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.9.3] - Current release

* Fixed a bug where Holographic remoting connection is not reliable
* Fixed a bug where the VR rendering performance is sub-optimum after upgrade to Unity's 1.1.1 OpenXR plugin.

## [0.9.2] - 2021-04-21

* Plane detection on HoloLens 2 in plugin version 0.9.1 will work with version 105 of the Mixed Reality OpenXR preview runtime.
* Plane detection on HoloLens 2 in plugin version 0.9.2 will work with version 106 of the Mixed Reality OpenXR preview runtime.
* Removed some unused callbacks from InputProvider to prevent calls like XRInputSubsystem.GetTrackingOriginMode (which aren't managed by our input system) from returning success with misleading values.
* Split out deprecated version of XRAnchorStore into its own file to prevent Unity console warning.

## [0.9.1] - 2021-04-20

* Depends on Unity's 1.1.1 OpenXR Plugin.
* Added support for [Holographic Remoting application](https://aka.ms/openxr-unity-app-remoting) for UWP platform.
* Fix UnityException where XRAnchorStore was trying to get a settings instance outside the main thread.

## [0.9.0] - 2021-03-29

* Added support for spatial mapping via XRMeshSubsystem and ARMeshManager.
* Added new C# API to get OpenXR handles to support other Unity packages consumes OpenXR extensions.
* Added new C# API to interop with Windows.Perception APIs to support other Unity packages consuming Perception WinRT APIs.
* Removed interaction profiles from required features in Windows Mixed Reality feature set, so developers can choose the motion controllers they tested with.
* Added Holographic editor remoting feature validator to help users to setup editor remoting properly.
* Fixed a bug where Unity editor crashes when exiting Holographic editor remoting mode after connection failure.
* Fixed a bug where unpremultipled alpha textures leads to sub-optimum performance on HoloLens 2.
* Fixed a bug where hand tracking was not located correctly when the scene origin was at floor level.
* Fixed a bug where hand mesh tracking disappear after leaving and loading a new scene.
* Fixed a bug where locatable camera provider didn't properly clean up.
* Revised the namespace of XRAnchorStore API into Microsoft.MixedReality.OpenXR.

## [0.2.0] - 2021-03-24

* Depends on Unity's 1.0.3 OpenXR Plugin.
* Removed deprecated preview APIs.
* Supports new API "EyeLevelSceneOrigin" for easily setup eye level experience for HoloLens 2.
* Supports plane detection using the ARPlaneSubsystem on HoloLens 2.
* Supports single raycasts for planes using the ARRaycastSubsystem on HoloLens 2.
* Supports new HandMeshTracker API for hand mesh tracking inputs on HoloLens 2.
* Fixed a bug where ARAnchor is not properly reporting tracking state.

## [0.1.5] - 2021-03-15

* Fixed a bug where using an HP Reverb G2 controller lead to errors in the Unity plugin.
* Fixed a bug that the Unity's "Input Debugger" window is blank when using Mixed Reality plugin.

## [0.1.4] - 2021-03-02

* Depends on Unity's 1.0.2 OpenXR Plugin.
* Fixed a bug where SpatialGraphNode's TryLocateSpace's FrameTime parameter was ignored.
* Fixed a bug where hand tracking could occasionally cause a crash.

## [0.1.3] - 2021-02-11

* Adds support for [desktop app holographic remoting](https://aka.ms/openxr-unity-app-remoting).
* Adds support for "SpatialGraphNode" API that bridges to other Mixed Reality tracking libraries, such as QR code tracking.
* Promote "FrameTime" concept from Preview API to supported API.
* Fixed a bug where eye tracking device capability is duplicated in manifest file.
* Fixed a bug where the plugin doesn't compile in Unity 2021.1+.

## [0.1.2] - 2021-01-08

* Depends on Unity's 0.1.2-preview.2
* Fixed unnecessary error message in XRAnchorStore before XR plugin is initialized.
* Fixed a bug where HandTracker's `TryLocateHandJoints` method might throw a `DllNotFoundException` if the DLL wasn't properly loaded. It now returns `false` instead.

## [0.1.1] - 2020-12-18

* Fixed a bug where non-existent sources were being reported disconnected on shutdown, possibly causing errors.
* Fixed a bug that the menu button on HP Reverb G2 didn't bind correctly.
* Changed the SetSceneOrigin script to focus on overriding eye level experience instead.
* Fixed a bug that returns incorrect room boundary on Mixed Reality headset.
* Fixed a bug where sample scene anchor scenarios didn't work with ARFoundation before 4.1.1.

## [0.1.0] - 2020-12-16

### Initial release

This is initial release of *Mixed Reality OpenXR Plugin \<com.microsoft.mixedreality.openxr\>*.

* Supports both UWP applications for Hololens 2 and Win32 VR applications for Windows Mixed Reality headsets.
* Optimizes UWP package and CoreWindow interaction for HoloLens 2 applications.
* Supports motion controller and hand interactions, including the new HP Reverb G2 controller.
* Supports articulated hand tracking using 26 joints and joint radius inputs.
* Supports eye gaze interaction on HoloLens 2.
* Supports locating PV camera on HoloLens 2.
* Supports mixed reality capture using 3rd eye rendering through PV camera.
* Supports "Play" to HoloLens 2 using Holographic Remoting app, allow developers to debug scripts without build and deploy to the device.
* Compatible with MRTK Unity 2.5.2 through MRTK OpenXR adapter package.
