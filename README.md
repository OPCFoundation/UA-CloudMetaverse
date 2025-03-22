# UA-CloudMetaverse

OPC UA Industrial Metaverse reference implementation leveraging Unity and the Digital Twin Consortium's Manufacturing Ontologies Reference Solution.

<img src="Docs/metaverse.png" alt="screenshot" width="900" />

## Prerequisits

1. A PC with at least 16 GB of memory and a modern graphics card with at least 6 GB of memory.
1. An OpenXR-compatible Virtual Reality headset. Make sure you enable `developer mode` on your headset, if required. The solution has already been pre-configured for a Meta Quest headset.
1. An Azure subscription. If you don't have one, you can start with a free subscription from [here](https://azure.microsoft.com/en-us/free).

## Installation & Running the Reference Implementation

1. Download this repository to a directory of your choice from [here](https://github.com/OPCFoundation/UA-CloudMetaverse/archive/refs/heads/main.zip).
1. Obtain a license for Unity from [here](https://store.unity.com/compare-plans).
1. Obtain a license for Cesium ion from [here](https://cesium.com/ion/signup/).
1. Install the latest Unity Hub version from [here](https://unity.com/download#how-get-started).
1. Install the latest Unity Editor version from [here](https://learn.unity.com/tutorial/install-the-unity-hub-and-editor).
1. Install the OpenXR plugin for Unity via Edit -> Project Settings in the Unity Editor.
1. Install the Cesium for Unity plugin via the Unity Package Manager > My Registries and enter `Cesium` for the Name, `https://unity.pkg.cesium.com` for the URL, `com.cesium.unity` for the Scope and click Save. Then, select the Cesium for Unity package and click Install.
1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) and the [Visual Studio tools for Unity](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/getting-started-with-visual-studio-tools-for-unity).
1. Install any additional OpenXR package for Unity you may need for your particular VR headset.
1. Install the Azure Industrial IoT Reference Solution from [here](https://learn.microsoft.com/en-us/azure/iot/tutorial-iot-industrial-solution-architecture#install-the-production-line-simulation-and-cloud-services).
1. Run the OPC UA simulation as described [here](https://learn.microsoft.com/en-us/azure/iot/tutorial-iot-industrial-solution-architecture#run-the-production-line-simulation).
1. Create an application registration for your ADX instance as described [here](https://docs.microsoft.com/en-us/azure/data-explorer/provision-azure-ad-app). Copy the client secret to a safe place.
1. In the C# file under the downloaded repository path /Assets/Scripts/SignalR/ADXService.cs, update the variables applicationClientId, adxInstanceURL, adxDatabaseName and tenantId with the data from your Azure Data Explorer instance, as well as from the new app registration you have just completed and from your Active Directory instance available in the Azure Portal.
1. Open the Unity project through the Unity Hub. In the Unity Editor select the ADTConnection in the Hierarchy panel. In the Inspector panel, provide the client secret copied previously in the Url field.
1. Select Cesium3DTileset in the Hierarchy panel and provide a Google Maps API key in the URL field in the form `https://tile.googleapis.com/v1/3dtiles/root.json?key=<your key>`. A Google Maps API key can be obtained from [here](https://console.cloud.google.com/google/maps-apis/start). Please make sure you enable the `Aerial View API` and the `Map Tiles API` in the Google Cloud Console.
1. Select the right platform for your Virtual Reality headset in Unity's `Build Settings`, select your headset in `Run Device`, select `Build And Run` and put on your headset.


