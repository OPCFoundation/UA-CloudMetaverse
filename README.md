# UA-CloudMetaverse

OPC UA Industrial Metaverse reference implementation leveraging Unity and the Digital Twin Consortium's Manufacturing Ontologies Reference Solution.

<img src="Docs/metaverse.png" alt="screenshot" width="900" />

## Installation & Running the Reference Implementation

1. Download this repository to a directory of your choice from [here](https://github.com/OPCFoundation/UA-CloudMetaverse/archive/refs/heads/main.zip).
1. Obtain a license for Unity from [here](https://store.unity.com/compare-plans).
1. Install the Unity Hub version 3.4.1 from [here](https://unity.com/download#how-get-started).
1. Install the Unity Editor version 2021.3.45f1 from [here](https://learn.unity.com/tutorial/install-the-unity-hub-and-editor).
1. Install Visual Studio 2022 and the [Visual Studio tools for Unity](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/getting-started-with-visual-studio-tools-for-unity).
1. Install the Digital Twin Consortium's Manufacturing Ontologies Reference Solution from [here](https://github.com/digitaltwinconsortium/ManufacturingOntologies#installation-of-production-line-simulation-and-cloud-services).
1. Run the OPC UA simulation as described [here](https://github.com/digitaltwinconsortium/ManufacturingOntologies#running-the-production-line-simulation).
1. Create an application registration for your ADX instance as described [here](https://docs.microsoft.com/en-us/azure/data-explorer/provision-azure-ad-app). Copy the client secret to a safe place.
1. In the C# file under the downloaded repository path /Assets/Scripts/SignalR/ADXService.cs, update the variables applicationClientId, adxInstanceURL, adxDatabaseName and tenantId with the data from our Azure Data Explorer instance, new app registration you have just completed and your Active Directory available in the Azure Portal.
1. Open the Unity project through the Unity Hub in the Unity Editor and select the ADTConnection in the Hirarchy panel. In the Inspector panel, provide the client secret copied previously in the Url field.
1. Put on your VR or AR headset and build and run the scene.


