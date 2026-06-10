using UnityEngine;

public class ScenarioCloseButton : MonoBehaviour
{
    public ScenarioUIToggle scenarioUIToggle;

    public void Interact()
    {
        Debug.Log("UI X ¹öÆ° Ray Å¬¸¯µÊ");

        if (scenarioUIToggle != null)
        {
            scenarioUIToggle.HideUI();
        }
    }
}