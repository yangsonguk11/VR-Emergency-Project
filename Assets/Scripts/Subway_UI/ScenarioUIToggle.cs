using UnityEngine;

public class ScenarioUIToggle : MonoBehaviour
{
    [Header("Scenario UI")]
    public GameObject scenarioUI;

    private void Start()
    {
        if (scenarioUI == null)
            scenarioUI = gameObject;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            ShowUI();
        }
    }

    // UI의 X 버튼 OnClick에 연결할 함수
    public void HideUI()
    {
        scenarioUI.SetActive(false);
    }

    public void ShowUI()
    {
        scenarioUI.SetActive(true);
    }
}