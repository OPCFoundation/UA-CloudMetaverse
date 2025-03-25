using UnityEngine;

public class Update : MonoBehaviour
{
    public TurbineSiteData siteData;
    public WindTurbineUIPanel turbinePanelDataVisContentPrefab;

    public void Start()
    {
        SetToNothing();
    }

    public void UpdateMethod(string name)
    {
        if (name == "-1")
        {
            SetToNothing();
        }
        else
        {
            turbinePanelDataVisContentPrefab.SetTurbineData(siteData.turbineData[int.Parse(name)]);
        }
    }

    private void SetToNothing()
    {
        WindTurbineScriptableObject turbine = ScriptableObject.CreateInstance<WindTurbineScriptableObject>();

        turbine.windTurbineData = new WindTurbineData()
        {
            AmbientTemperature = 0,
            Power = 0,
            RotorSpeed = 0,
            TurbineId = string.Empty,
            WindSpeed = 0
        };

        turbinePanelDataVisContentPrefab.SetTurbineData(turbine);
    }
}
