using UnityEngine;

public class ScenarioUIToggle : MonoBehaviour
{
    [Header("Scenario UI")]
    public GameObject scenarioUI;

    private void Start()
    {
        if (scenarioUI == null)
        {
            scenarioUI = gameObject;
        }
    }

    private void Update()
    {
        // B ¹öÆ°
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            ToggleUI();
        }
    }

    public void ToggleUI()
    {
        scenarioUI.SetActive(!scenarioUI.activeSelf);
    }
}