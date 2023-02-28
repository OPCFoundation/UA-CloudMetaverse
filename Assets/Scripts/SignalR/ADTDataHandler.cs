// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Unity;
using System.Threading.Tasks;
using UnityEngine;

public class ADTDataHandler : MonoBehaviour
{
    private ADXService rService;

    public string url = "";
    public TurbineSiteData turbineSiteData;
    public WindTurbineGameEvent TurbinePropertyMessageReceived;

    private void Start()
    {
        this.RunSafeVoid(CreateServiceAsync);
    }

    private void OnDestroy()
    {
        if (rService != null)
        {
            rService.OnTelemetryMessage -= HandleTelemetryMessage;
        }
    }

    /// <summary>
    /// Received a message from SignalR. Note, this message is received on a background thread.
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    private void HandleTelemetryMessage(TelemetryMessage message)
    {
        // Finally update Unity GameObjects, but this must be done on the Unity Main thread.
        UnityDispatcher.InvokeOnAppThread(() =>
        {
            foreach (WindTurbineScriptableObject turbine in turbineSiteData.turbineData)
            {
                if (turbine.windTurbineData.TurbineId == message.TurbineID)
                {
                    turbine.UpdateData(CreateNewWindTurbineData(message));
                    return;
                }
            }
        });
    }

    /// <summary>
    /// Construct the WindTurbine Data received from SignalR
    /// </summary>
    /// <param name="message">Telemetry data</param>
    /// <returns>Data values of wind turbine</returns>
    private WindTurbineData CreateNewWindTurbineData(TelemetryMessage message)
    {
        WindTurbineData data = new WindTurbineData
        {
            TurbineId = message.TurbineID,
            AmbientTemperature = message.Ambient,
            EventCode = message.Code,
            EventDescription = message.Description,
            Power = message.Power,
            RotorSpeed = message.Rotor,
            TimeInterval = message.TimeInterval,
            WindSpeed = message.WindSpeed,
        };

        return data;
    }

    private Task CreateServiceAsync()
    {
        rService = new ADXService();
        rService.OnTelemetryMessage += HandleTelemetryMessage;
        return rService.StartAsync(url);
    }
}