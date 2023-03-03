# UA-CloudMetaverse

OPC UA Industrial Metaverse reference implementation leveraging Unity and the Digital Twin Consortium's Manufacturing Ontologies Reference Solution.

<img src="Docs/metaverse.png" alt="screenshot" width="900" />

## Prerequisits

1. A PC with at least 16 GB of memory and a modern graphics card with at least 6 GB of memory.
1. A Microsoft Mixed Reality Toolkit-compatible Virtual Reality or Augmented Reality headset.
1. An Azure subscription. If you don't have one, you can start with a free subscription from [here](https://azure.microsoft.com/en-us/free).

## Installation & Running the Reference Implementation

1. Download this repository to a directory of your choice from [here](https://github.com/OPCFoundation/UA-CloudMetaverse/archive/refs/heads/main.zip).
1. Obtain a license for Unity from [here](https://store.unity.com/compare-plans).
1. Install the Unity Hub version 3.4.1 from [here](https://unity.com/download#how-get-started).
1. Install the Unity Editor version 2021.3.45f1 from [here](https://learn.unity.com/tutorial/install-the-unity-hub-and-editor).
1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) and the [Visual Studio tools for Unity](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/getting-started-with-visual-studio-tools-for-unity).
1. Install the Digital Twin Consortium's Manufacturing Ontologies Reference Solution from [here](https://github.com/digitaltwinconsortium/ManufacturingOntologies#installation-of-production-line-simulation-and-cloud-services).
1. Run the OPC UA simulation as described [here](https://github.com/digitaltwinconsortium/ManufacturingOntologies#running-the-production-line-simulation).
1. Create an application registration for your ADX instance as described [here](https://docs.microsoft.com/en-us/azure/data-explorer/provision-azure-ad-app). Copy the client secret to a safe place.
1. In the C# file under the downloaded repository path /Assets/Scripts/SignalR/ADXService.cs, update the variables applicationClientId, adxInstanceURL, adxDatabaseName and tenantId with the data from our Azure Data Explorer instance, new app registration you have just completed and your Active Directory available in the Azure Portal.
1. Open the Unity project through the Unity Hub, in the Unity Editor select the ADTConnection in the Hierarchy panel. In the Inspector panel, provide the client secret copied previously in the Url field.
1. Select Bing Maps Operate in the Hierarchy panel and provide a Bing Maps SDK key in the Developer Key field. A Bing Maps SDK key can be obtained from [here](https://www.bingmapsportal.com/).
1. Put on your Virtual Reality or Augmented Reality headset and build and run the scene.


