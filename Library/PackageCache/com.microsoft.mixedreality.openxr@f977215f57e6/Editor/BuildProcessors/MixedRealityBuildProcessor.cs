// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    // Customize code for UWP (aka WSA platform)
    // Enable the "holographic window attachment" and slightly modify the app.cpp file in app.
    internal class MixedRealityBuildProcessor : OpenXRFeatureBuildHooks
    {
        private const string MixedRealityPluginName = "MicrosoftOpenXRPlugin.dll";
        private const string RemotingPluginName = "Microsoft.Holographic.AppRemoting.OpenXr.dll";
        private const string RemotingJsonName = "RemotingXR.json";
        private const string RemotingJsonGuid = "db1217138e9d063459fa78b3e75f4f93";

        private static readonly Dictionary<string, string> BootVars = new Dictionary<string, string>()
        {
            {"force-primary-window-holographic", "1"},
            {"vr-enabled", "1"},
            {"xrsdk-windowsmr-library", NativeLib.DllName + ".dll"},
            {"early-boot-windows-holographic", "1"},
        };

        public override void OnPreprocessBuild(BuildReport report)
        {
            base.OnPreprocessBuild(report);

            PluginImporter[] allPlugins = PluginImporter.GetAllImporters();
            foreach (PluginImporter plugin in allPlugins)
            {
                if (plugin.isNativePlugin)
                {
                    if (plugin.assetPath.Contains(MixedRealityPluginName))
                    {
                        plugin.SetIncludeInBuildDelegate(IsHoloLensFeatureEnabled);
                    }
                    else if (plugin.assetPath.Contains(RemotingPluginName))
                    {
                        plugin.SetIncludeInBuildDelegate(IsAppRemotingEnabled);
                    }
                }
            }
        }

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
            if (report.summary.platformGroup == BuildTargetGroup.WSA)
            {
                PreprocessBuildForWSA(report);
            }
        }

        private void PreprocessBuildForWSA(BuildReport report)
        {
            // Write boot settings before build
            BootConfig bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            foreach (KeyValuePair<string, string> entry in BootVars)
            {
                if (entry.Key == "force-primary-window-holographic" && IsAppRemotingEnabled(string.Empty)){
                    // When AppRemoting is enabled, skip the flag to force primary corewindow to be holographic (it won't be).
                    // If this flag exist, Unity might hit a bug that it skips rendering into the CoreWindow on the desktop.
                    continue;

                }
                bootConfig.SetValueForKey(entry.Key, entry.Value);
            }

            bootConfig.WriteBootConfig();
        }

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
            if (report.summary.platformGroup == BuildTargetGroup.WSA)
            {
                PostprocessBuildForWSA(report);
            }

            CheckRemotingJson(report, IsAppRemotingEnabled(string.Empty));
        }

        private void PostprocessBuildForWSA(BuildReport report)
        {
            // Clean up boot settings after build
            BootConfig bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            foreach (KeyValuePair<string, string> entry in BootVars)
            {
                bootConfig.ClearEntryForKeyAndValue(entry.Key, entry.Value);
            }

            bootConfig.WriteBootConfig();

            if (IsAppRemotingEnabled(string.Empty))
            {
                AddRemotingJsonToData(Path.Combine(report.summary.outputPath, PlayerSettings.productName));
            }

            RemoveSuppressSystemOverlays(report);
        }

        /// <summary>
        /// Copies RemotingXR.json to or deletes from the build folder, depending on shouldExist.
        /// </summary>
        /// <param name="report">The build report from Unity's build hooks events.</param>
        /// <param name="shouldExist">If the file should be present in the build.</param>
        private void CheckRemotingJson(BuildReport report, bool shouldExist)
        {
            string path = report.summary.outputPath;

            if (report.summary.platform == BuildTarget.WSAPlayer)
            {
                path = Path.Combine(path, PlayerSettings.productName);
                path = Path.Combine(path, RemotingJsonName);
            }
            else if (report.summary.platform == BuildTarget.StandaloneWindows64)
            {
                path = Path.Combine(path, "..");
                path = Path.Combine(path, PlayerSettings.productName + "_Data");
                path = Path.Combine(path, "Plugins");
                path = Path.Combine(path, "x86_64");
                path = Path.Combine(path, RemotingJsonName);
            }
            else
            {
                // Other platforms aren't supported
                return;
            }

            if (shouldExist)
            {
                File.Copy(Path.GetFullPath(AssetDatabase.GUIDToAssetPath(RemotingJsonGuid)), path, true);
            }
            else
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Adds a line in the Unity data file project to include the remoting file in the build.
        /// </summary>
        /// <param name="path">The path to the folder that contains Unity Data.vcxitems.</param>
        private void AddRemotingJsonToData(string path)
        {
            const string UnityDataPath = "Unity Data.vcxitems";
            path = Path.Combine(path, UnityDataPath);

            XElement root = XElement.Load(path);
            foreach (XElement itemGroup in root.Elements(root.GetDefaultNamespace() + "ItemGroup"))
            {
                foreach (XElement remotingDll in itemGroup.Elements(root.GetDefaultNamespace() + "None"))
                {
                    XAttribute includeDll = remotingDll.Attribute("Include");
                    if (includeDll != null && includeDll.Value.Contains(RemotingPluginName))
                    {
                        XElement jsonElement = new XElement(remotingDll);
                        // Update "Include" to point to the json, but leave "Condition" alone so it's still dependent on the remoting binary existing
                        jsonElement.Attribute("Include").Value = jsonElement.Attribute("Include").Value.Replace(RemotingPluginName, RemotingJsonName);
                        itemGroup.Add(jsonElement);
                        root.Save(path);
                        return;
                    }
                }
            }
        }

        // Remove the deprecated usage of SuppressSystemOverlays from Unity UWP project template.
        private void RemoveSuppressSystemOverlays(BuildReport report)
        {
            string appCppPath = Path.Combine(report.summary.outputPath, PlayerSettings.productName, "App.cpp");
            string appCppLines = File.ReadAllText(appCppPath);
            const string Pattern = @"\r?\n.*SuppressSystemOverlays.*\r?\n";
            string modifiedAppCppLines = System.Text.RegularExpressions.Regex.Replace(appCppLines, Pattern, "");
            File.WriteAllText(appCppPath, modifiedAppCppLines);
        }

        private bool IsAppRemotingEnabled(string path) => CheckFeatureEnabled<AppRemotingPlugin>();
        private bool IsHoloLensFeatureEnabled(string path) => CheckFeatureEnabled<HoloLensFeaturePlugin>() || CheckFeatureEnabled<HandTrackingFeaturePlugin>();

        /// <summary>
        /// Checks the OpenXRSettings for a specific feature to be enabled.
        /// </summary>
        /// <typeparam name="T">The specific feature to check.</typeparam>
        /// <returns>Whether the feature is enabled or not.</returns>
        private bool CheckFeatureEnabled<T>() where T : OpenXRFeaturePlugin<T>
        {
            EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out UnityEngine.Object obj);
            OpenXRSettings openXRSettings = null;
            if (obj is IPackageSettings packageSettings)
            {
                openXRSettings = packageSettings.GetSettingsForBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            }

            if (openXRSettings != null)
            {
                foreach (OpenXRFeature feature in openXRSettings.GetFeatures())
                {
                    if (feature is T)
                    {
                        return feature.enabled;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Small utility class for reading, updating and writing boot config.
        /// </summary>
        private class BootConfig
        {
            private const string XrBootSettingsKey = "xr-boot-settings";

            private readonly Dictionary<string, string> bootConfigSettings;
            private readonly string buildTargetName;

            public BootConfig(BuildReport report)
            {
                bootConfigSettings = new Dictionary<string, string>();
                buildTargetName = BuildPipeline.GetBuildTargetName(report.summary.platform);
            }

            public void ReadBootConfig()
            {
                bootConfigSettings.Clear();

                string xrBootSettings = EditorUserBuildSettings.GetPlatformSettings(buildTargetName, XrBootSettingsKey);
                if (!string.IsNullOrEmpty(xrBootSettings))
                {
                    // boot settings string format
                    // <boot setting>:<value>[;<boot setting>:<value>]*
                    var bootSettings = xrBootSettings.Split(';');
                    foreach (var bootSetting in bootSettings)
                    {
                        var setting = bootSetting.Split(':');
                        if (setting.Length == 2 && !string.IsNullOrEmpty(setting[0]) && !string.IsNullOrEmpty(setting[1]))
                        {
                            bootConfigSettings.Add(setting[0], setting[1]);
                        }
                    }
                }
            }

            public void SetValueForKey(string key, string value) => bootConfigSettings[key] = value;

            public void ClearEntryForKeyAndValue(string key, string value)
            {
                if (bootConfigSettings.TryGetValue(key, out string dictValue) && dictValue == value)
                {
                    bootConfigSettings.Remove(key);
                }
            }

            public void WriteBootConfig()
            {
                // boot settings string format
                // <boot setting>:<value>[;<boot setting>:<value>]*
                bool firstEntry = true;
                var sb = new System.Text.StringBuilder();
                foreach (var kvp in bootConfigSettings)
                {
                    if (!firstEntry)
                    {
                        sb.Append(";");
                    }
                    sb.Append($"{kvp.Key}:{kvp.Value}");
                    firstEntry = false;
                }

                EditorUserBuildSettings.SetPlatformSettings(buildTargetName, XrBootSettingsKey, sb.ToString());
            }
        }

        public override int callbackOrder => 1;
        public override Type featureType => typeof(HoloLensFeaturePlugin);

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
        }
    }
}