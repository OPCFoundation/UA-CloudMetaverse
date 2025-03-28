// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections;
using UnityEngine;

/// <summary>
/// WindTurbineUIPanel uses the scriptable object to update relevant UI
/// </summary>
public class WindTurbineUIPanel : MonoBehaviour
{
    /// <summary>
    /// Wind Turbine Data
    /// </summary>
    [SerializeField]
    private WindTurbineScriptableObject windTurbineData;

    [SerializeField]
    private bool animateValueTransitions = true;

    [SerializeField]
    private float valueTransitionTime = 2f;

    [SerializeField]
    private TurbineLineRendererConnector lineRendererConnector;

    [SerializeField]
    private WindTurbineGameEvent onResetCommandSent;

    public ProgressController progressControllerPower;
    public ProgressController progressControllerPowerBar;
    public ProgressController progressControllerTemperature;
    public ProgressController progressControllerWindSpeed;
    public ProgressController progressControllerRotorSpeed;

    private IEnumerator currentAnimation;

    /// <summary>
    /// Setup the on data updated callback.
    /// </summary>
    private void OnEnable()
    {
        if (windTurbineData)
        {
            SetTurbineData(windTurbineData);
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromUpdate();
    }

    /// <summary>
    /// Sets the ScriptableObject containing the info for this panel.
    /// Will refresh all UI values
    /// </summary>
    /// <param name="turbineData"></param>
    public void SetTurbineData(WindTurbineScriptableObject turbineData)
    {
        UnsubscribeFromUpdate();
        windTurbineData = turbineData;
        windTurbineData.onDataUpdated += OnWindTurbineDataUpdated;
        OnWindTurbineDataUpdated();
        if (lineRendererConnector)
        {
            lineRendererConnector.SetTarget(turbineData);
        }
    }

    public void SendResetCommand()
    {
        onResetCommandSent.Raise(windTurbineData);
    }

    /// <summary>
    /// Update relevant UI based on new data.
    /// </summary>
    private void OnWindTurbineDataUpdated()
    {
        if (animateValueTransitions)
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            currentAnimation = AnimatePanelValues();
            StartCoroutine(currentAnimation);
        }
        else
        {
            SetUIValues();
        }
    }

    private IEnumerator AnimatePanelValues()
    {
        var startTime = Time.time;
        var elapsedTime = 0f;

        while (elapsedTime < valueTransitionTime)
        {
            elapsedTime = Time.time - startTime;
            var t = elapsedTime / valueTransitionTime;

            progressControllerPower.CurrentValue = Mathf.Lerp(
                (float)progressControllerPower.CurrentValue,
                (float)windTurbineData.windTurbineData.Power, t);

            progressControllerTemperature.CurrentValue = Mathf.Lerp(
                (float)progressControllerTemperature.CurrentValue,
                (float)windTurbineData.windTurbineData.AmbientTemperature, t);

            progressControllerWindSpeed.CurrentValue = Mathf.Lerp(
                (float)progressControllerWindSpeed.CurrentValue,
                (float)windTurbineData.windTurbineData.WindSpeed, t);

            progressControllerRotorSpeed.CurrentValue = Mathf.Lerp(
                (float)progressControllerRotorSpeed.CurrentValue,
                (float)windTurbineData.windTurbineData.RotorSpeed, t);

            yield return null;
        }

        SetUIValues();
    }

    private void SetUIValues()
    {
        progressControllerPowerBar.CurrentValue = windTurbineData.windTurbineData.Power;
        progressControllerTemperature.CurrentValue = windTurbineData.windTurbineData.AmbientTemperature;
        progressControllerWindSpeed.CurrentValue = windTurbineData.windTurbineData.WindSpeed;
        progressControllerRotorSpeed.CurrentValue = windTurbineData.windTurbineData.RotorSpeed;
    }

    private void UnsubscribeFromUpdate()
    {
        if (windTurbineData != null)
        {
            windTurbineData.onDataUpdated -= OnWindTurbineDataUpdated;
        }
    }
}